﻿using System;
using RabbitMQ.Client;

namespace EvenBus.Extensions.EventBus.RabbitMQPersistent
{
    /// <summary>
    ///     RabbitMQ持久连接
    ///     接口
    /// </summary>
    public interface IRabbitMQPersistentConnection
        : IDisposable
    {
        bool IsConnected { get; }

        bool TryConnect();

        IModel CreateModel();
    }
}