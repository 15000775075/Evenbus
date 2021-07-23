namespace Common.Config
{
    /// <summary>
    ///     应用配置
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        ///     ConnectionStrings
        /// </summary>
        public ConnectionStrings ConnectionStrings { get; set; }

        /// <summary>
        ///     Redis
        /// </summary>
        public Redis Redis { get; set; }


        /// <summary>
        ///     RabbitMQ
        /// </summary>
        public RabbitMQ RabbitMQ { get; set; }


        public EventBus EventBus { get; set; }
    }

    /// <summary>
    /// </summary>
    public class ConnectionStrings
    {
        /// <summary>
        ///     启用
        /// </summary>
        public string DefaultConnection { get; set; }

        /// <summary>
        ///     地址
        /// </summary>
        public string BigPlatformConnection { get; set; }
    }

    /// <summary>
    /// </summary>
    public class Redis
    {
        /// <summary>
        ///     事物
        /// </summary>
        public string ConnectionString { get; set; }
    }


    public class RabbitMQ
    {
        public bool Enabled { get; set; }
        public string Connection { get; set; }

        public string UserName { get; set; }
        public string Password { get; set; }

        public string RetryCount { get; set; }
    }

    public class EventBus
    {
        public bool Enabled { get; set; }
        public string SubscriptionClientName { get; set; }

    }

}