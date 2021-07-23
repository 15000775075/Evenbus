using System;

namespace EvenBus.Extensions.EventBus.EventBusSubscriptions
{
    /// <summary>
    ///     订阅信息模型
    /// </summary>
    public class SubscriptionInfo
    {
        private SubscriptionInfo(bool isDynamic, Type handlerType)
        {
            IsDynamic = isDynamic;
            HandlerType = handlerType;
        }

        public bool IsDynamic { get; }
        public Type HandlerType { get; }

        public static SubscriptionInfo Dynamic(Type handlerType)
        {
            return new(true, handlerType);
        }

        public static SubscriptionInfo Typed(Type handlerType)
        {
            return new(false, handlerType);
        }
    }
}