using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using Common.Config;
using EvenBus.Extensions.EventBus.RabbitMQPersistent;

namespace Evenbus.Extensions
{
    public static class RabbitMQExtension
    {
        public static void AddRabbitMQ(this IServiceCollection services, AppSettings appSettings)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (appSettings.RabbitMQ.Enabled)
            {
                services.AddSingleton<IRabbitMQPersistentConnection>(sp =>
                {
                    ILogger<RabbitMQPersistentConnection> logger = sp.GetRequiredService<ILogger<RabbitMQPersistentConnection>>();

                    ConnectionFactory factory = new ConnectionFactory()
                    {
                        HostName = appSettings.RabbitMQ.Connection,
                        DispatchConsumersAsync = true
                    };

                    if (!string.IsNullOrEmpty(appSettings.RabbitMQ.UserName))
                    {
                        factory.UserName = appSettings.RabbitMQ.UserName;
                    }

                    if (!string.IsNullOrEmpty(appSettings.RabbitMQ.Password))
                    {
                        factory.Password = appSettings.RabbitMQ.Password;
                    }

                    int retryCount = 5;
                    if (!string.IsNullOrEmpty(appSettings.RabbitMQ.RetryCount))
                    {
                        retryCount = int.Parse(appSettings.RabbitMQ.RetryCount);
                    }

                    return new RabbitMQPersistentConnection(factory, logger, retryCount);
                });
            }
        }
    }
}