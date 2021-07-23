namespace EvenBus.Extensions.EventHandling
{
    public class SelectIntegrationEvent : IntegrationEvent
    {
        public SelectIntegrationEvent(string id)
        {
            Id = id;
        }

        public new string Id { get; }
    }
}