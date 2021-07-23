using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using Common.Config;
using EvenBus.Extensions;
using EvenBus.Extensions.EventBus.EventBusSubscriptions;
using EvenBus.Extensions.RabbitMQPersistent;
using EvenBus.Extensions.EventBus.RabbitMQPersistent;
using EvenBus.Extensions.EventHandling;

namespace Evenbus.Extensions
{
    /// <summary>
    /// EventBus 事件总线服务
    /// </summary>
    public static class EventBusExtension
    {
        public static void AddEventBus(this IServiceCollection services, AppSettings appSettings)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var subscriptionClientName = appSettings.EventBus.SubscriptionClientName;


            services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();
            services.AddTransient<DeletedIntegrationEventHandler>();
            services.AddTransient<SelectIntegrationEventHandler>();
            services.AddTransient<DeletedIntegrationEventHandlerDir>();


            services.AddSingleton<IEventBus, EventBusRabbitMQ>(sp =>
            {
                var rabbitMQPersistentConnection = sp.GetRequiredService<IRabbitMQPersistentConnection>();
                ILifetimeScope iLifetimeScope = sp.GetRequiredService<ILifetimeScope>();
                ILogger<EventBusRabbitMQ> logger = sp.GetRequiredService<ILogger<EventBusRabbitMQ>>();
                var eventBusSubcriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();

                int retryCount = 5;
                if (!string.IsNullOrEmpty(appSettings.RabbitMQ.RetryCount))
                {
                    retryCount = int.Parse(appSettings.RabbitMQ.RetryCount);
                }

                return new EventBusRabbitMQ(rabbitMQPersistentConnection, logger, iLifetimeScope, eventBusSubcriptionsManager, subscriptionClientName, retryCount);
            });

        }


        public static void ConfigureEventBus(this IApplicationBuilder app, AppSettings appSettings)
        {

            var eventBus = app.ApplicationServices.GetRequiredService<IEventBus>();
             //eventBus.Subscribe<DeletedIntegrationEvent, DeletedIntegrationEventHandler>();
           // eventBus.SubscribeDynamic <.SelectIntegrationEventHandler>("SelectIntegrationEvent");
            eventBus.Subscribe<SelectIntegrationEvent, SelectIntegrationEventHandler>();
             eventBus.SubscribeDynamic <DeletedIntegrationEventHandlerDir>("DeletedIntegrationEvent");

        }
    }
}