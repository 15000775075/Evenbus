﻿using Autofac;
using EvenBus.Extensions.EventBus.EventBusSubscriptions;
using EvenBus.Extensions.EventBus.RabbitMQPersistent;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Common.Extensions;
using Common.Helper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace EvenBus.Extensions.RabbitMQPersistent
{
    /// <summary>
    ///     基于RabbitMQ的事件总线
    /// </summary>
    public class EventBusRabbitMQ : IEventBus, IDisposable
    {
        private const string BROKER_NAME = "admin_event_bus";
        private readonly ILifetimeScope _autofac;
        private readonly ILogger<EventBusRabbitMQ> _logger;

        private readonly IRabbitMQPersistentConnection _persistentConnection;
        private readonly int _retryCount;
        private readonly IEventBusSubscriptionsManager _subsManager;
        private readonly string AUTOFAC_SCOPE_NAME = "admin_event_bus";

        private IModel _consumerChannel;
        private string _queueName;

        /// <summary>
        ///     RabbitMQ事件总线
        /// </summary>
        /// <param name="persistentConnection">RabbitMQ持久连接</param>
        /// <param name="logger">日志</param>
        /// <param name="autofac">autofac容器</param>
        /// <param name="subsManager">事件总线订阅管理器</param>
        /// <param name="queueName">队列名称</param>
        /// <param name="retryCount">重试次数</param>
        public EventBusRabbitMQ(IRabbitMQPersistentConnection persistentConnection, ILogger<EventBusRabbitMQ> logger,
            ILifetimeScope autofac,
            IEventBusSubscriptionsManager subsManager,
            string queueName = null,
            int retryCount = 5)
        {
            _persistentConnection =
                persistentConnection ?? throw new ArgumentNullException(nameof(persistentConnection));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _subsManager = subsManager ?? new InMemoryEventBusSubscriptionsManager();
            _queueName = queueName;
            _consumerChannel = CreateConsumerChannel();
            _autofac = autofac;
            _retryCount = retryCount;
            _subsManager.OnEventRemoved += SubsManager_OnEventRemoved;
        }

        public void Dispose()
        {
            if (_consumerChannel != null)
            {
                _consumerChannel.Dispose();
            }

            _subsManager.Clear();
        }

        /// <summary>
        ///     发布
        /// </summary>
        /// <param name="event">事件模型</param>
        public void Publish(IntegrationEvent @event)
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            Polly.Retry.RetryPolicy policy = Policy.Handle<BrokerUnreachableException>()
                .Or<SocketException>()
                .WaitAndRetry(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (ex, time) =>
                    {
                        _logger.LogWarning(ex,
                            "Could not publish event: {EventId} after {Timeout}s ({ExceptionMessage})", @event.Id,
                            $"{time.TotalSeconds:n1}", ex.Message);
                    });

            string eventName = @event.GetType().Name;

            //_logger.LogTrace("Creating RabbitMQ channel to publish event: {EventId} ({EventName})", @event.Id,
            //    eventName);

            ConsoleHelper.WriteWarningLine($"Creating RabbitMQ channel to publish event: {@event.Id} ({eventName})");
            using (IModel channel = _persistentConnection.CreateModel())
            {
                _logger.LogTrace("Declaring RabbitMQ exchange to publish event: {EventId}", @event.Id);

                channel.ExchangeDeclare(BROKER_NAME, "direct");

                string message = JsonConvert.SerializeObject(@event);
                byte[] body = Encoding.UTF8.GetBytes(message);

                policy.Execute(() =>
                {
                    IBasicProperties properties = channel.CreateBasicProperties();
                    properties.DeliveryMode = 2; // persistent

                    _logger.LogTrace("Publishing event to RabbitMQ: {EventId}", @event.Id);

                    channel.BasicPublish(
                        BROKER_NAME,
                        eventName,
                        true,
                        properties,
                        body);
                });
            }
        }

        /// <summary>
        ///     订阅
        ///     动态
        /// </summary>
        /// <typeparam name="TH">事件处理器</typeparam>
        /// <param name="eventName">事件名</param>
        public void SubscribeDynamic<TH>(string eventName)
            where TH : IDynamicIntegrationEventHandler
        {
            _logger.LogInformation("Subscribing to dynamic event {EventName} with {EventHandler}", eventName,
                typeof(TH).GetGenericTypeName());

            DoInternalSubscription(eventName);
            _subsManager.AddDynamicSubscription<TH>(eventName);
            StartBasicConsume();
        }

        /// <summary>
        ///     订阅
        /// </summary>
        /// <typeparam name="T">约束：事件模型</typeparam>
        /// <typeparam name="TH">约束：事件处理器<事件模型></typeparam>
        public void Subscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            string eventName = _subsManager.GetEventKey<T>();
            DoInternalSubscription(eventName);

            _logger.LogInformation("Subscribing to event {EventName} with {EventHandler}", eventName,
                typeof(TH).GetGenericTypeName());

            $"Subscribing to event {eventName} with {typeof(TH).GetGenericTypeName()}".WriteSuccessLine();

            _subsManager.AddSubscription<T, TH>();
            StartBasicConsume();
        }

        /// <summary>
        ///     取消订阅
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TH"></typeparam>
        public void Unsubscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            string eventName = _subsManager.GetEventKey<T>();

            _logger.LogInformation("Unsubscribing from event {EventName}", eventName);

            _subsManager.RemoveSubscription<T, TH>();
        }

        public void UnsubscribeDynamic<TH>(string eventName)
            where TH : IDynamicIntegrationEventHandler
        {
            _subsManager.RemoveDynamicSubscription<TH>(eventName);
        }

        /// <summary>
        ///     订阅管理器事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventName"></param>
        private void SubsManager_OnEventRemoved(object sender, string eventName)
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            using (IModel channel = _persistentConnection.CreateModel())
            {
                channel.QueueUnbind(_queueName,
                    BROKER_NAME,
                    eventName);

                if (_subsManager.IsEmpty)
                {
                    _queueName = string.Empty;
                    _consumerChannel.Close();
                }
            }
        }

        private void DoInternalSubscription(string eventName)
        {
            bool containsKey = _subsManager.HasSubscriptionsForEvent(eventName);
            if (!containsKey)
            {
                if (!_persistentConnection.IsConnected)
                {
                    _persistentConnection.TryConnect();
                }

                using (IModel channel = _persistentConnection.CreateModel())
                {
                    channel.QueueBind(_queueName,
                        BROKER_NAME,
                        eventName);
                }
            }
        }

        /// <summary>
        ///     开始基本消费
        /// </summary>
        private void StartBasicConsume()
        {
            _logger.LogTrace("Starting RabbitMQ basic consume");

            if (_consumerChannel != null)
            {
                AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(_consumerChannel);

                consumer.Received += Consumer_Received;

                _consumerChannel.BasicConsume(
                    _queueName,
                    false,
                    consumer);
            }
            else
            {
                _logger.LogError("StartBasicConsume can't call on _consumerChannel == null");
            }
        }

        /// <summary>
        ///     消费者接受到
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        /// <returns></returns>
        private async Task Consumer_Received(object sender, BasicDeliverEventArgs eventArgs)
        {
            string eventName = eventArgs.RoutingKey;
            string message = Encoding.UTF8.GetString(eventArgs.Body.Span);

            try
            {
                if (message.ToLowerInvariant().Contains("throw-fake-exception"))
                {
                    throw new InvalidOperationException($"Fake exception requested: \"{message}\"");
                }

                await ProcessEvent(eventName, message);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "----- ERROR Processing message \"{Message}\"", message);
            }

            // Even on exception we take the message off the queue.
            // in a REAL WORLD app this should be handled with a Dead Letter Exchange (DLX). 
            // For more information see: https://www.rabbitmq.com/dlx.html
            _consumerChannel.BasicAck(eventArgs.DeliveryTag, false);
        }

        /// <summary>
        ///     创造消费通道
        /// </summary>
        /// <returns></returns>
        private IModel CreateConsumerChannel()
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            _logger.LogTrace("Creating RabbitMQ consumer channel");

            IModel channel = _persistentConnection.CreateModel();

            channel.ExchangeDeclare(BROKER_NAME,
                "direct");

            channel.QueueDeclare(_queueName,
                true,
                false,
                false,
                null);

            channel.CallbackException += (sender, ea) =>
            {
                _logger.LogWarning(ea.Exception, "Recreating RabbitMQ consumer channel");

                _consumerChannel.Dispose();
                _consumerChannel = CreateConsumerChannel();
                StartBasicConsume();
            };

            return channel;
        }

        private async Task ProcessEvent(string eventName, string message)
        {
            _logger.LogTrace("Processing RabbitMQ event: {EventName}", eventName);

            if (_subsManager.HasSubscriptionsForEvent(eventName))
            {
                using (ILifetimeScope scope = _autofac.BeginLifetimeScope(AUTOFAC_SCOPE_NAME))
                {
                    System.Collections.Generic.IEnumerable<SubscriptionInfo> subscriptions = _subsManager.GetHandlersForEvent(eventName);
                    foreach (SubscriptionInfo subscription in subscriptions)
                    {
                        if (subscription.IsDynamic)
                        {
                            IDynamicIntegrationEventHandler handler =
                                scope.ResolveOptional(subscription.HandlerType) as IDynamicIntegrationEventHandler;
                            if (handler == null)
                            {
                                continue;
                            }

                            dynamic eventData = JObject.Parse(message);

                            await Task.Yield();
                            await handler.Handle(eventData);
                        }
                        else
                        {
                            object handler = scope.ResolveOptional(subscription.HandlerType);
                            if (handler == null)
                            {
                                continue;
                            }

                            Type eventType = _subsManager.GetEventTypeByName(eventName);
                            object integrationEvent = JsonConvert.DeserializeObject(message, eventType);
                            Type concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);

                            await Task.Yield();
                            await (Task)concreteType.GetMethod("Handle").Invoke(handler, new[] { integrationEvent });
                        }
                    }
                }
            }
            else
            {
                _logger.LogWarning("No subscription for RabbitMQ event: {EventName}", eventName);
            }
        }
    }
}