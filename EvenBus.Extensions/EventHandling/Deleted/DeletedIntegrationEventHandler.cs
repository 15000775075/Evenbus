using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Common.Helper;

namespace EvenBus.Extensions.EventHandling
{
    public class DeletedIntegrationEventHandler : IIntegrationEventHandler<DeletedIntegrationEvent>
    {
        private readonly ILogger<DeletedIntegrationEventHandler> _logger;

        public DeletedIntegrationEventHandler(
            ILogger<DeletedIntegrationEventHandler> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task Handle(DeletedIntegrationEvent @event)
        {
            ConsoleHelper.WriteSuccessLine("---------------------------------------------------------------------------------------------------------------------------");
            _logger.LogInformation("----- Handling integration event: {IntegrationEventId} at {AppName} - ({@IntegrationEvent})", @event.Id, "admin", @event);
            ConsoleHelper.WriteSuccessLine($"----- Handling integration event: {@event.Id} at admin - ({@event})");
            return Task.FromResult("1");
        }

    }
}
