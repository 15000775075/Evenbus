namespace EvenBus.Extensions.EventHandling
{
    public class DeletedIntegrationEvent : IntegrationEvent
    {
        public DeletedIntegrationEvent(string id)
        {
            Id = id;
        }

        public new string Id { get; }
    }
}