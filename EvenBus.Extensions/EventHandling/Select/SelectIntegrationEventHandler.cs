using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Common.Helper;

namespace EvenBus.Extensions.EventHandling
{
    public class SelectIntegrationEventHandler : IIntegrationEventHandler<SelectIntegrationEvent>
    {
        private readonly ILogger<SelectIntegrationEventHandler> _logger;

        public SelectIntegrationEventHandler(
            ILogger<SelectIntegrationEventHandler> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task Handle(SelectIntegrationEvent @event)
        {
            ConsoleHelper.WriteSuccessLine("---------------------------------------------------------------------------------------------------------------------------");

            _logger.LogInformation("----- Handling integration event: {IntegrationEventId} at {AppName} - ({@IntegrationEvent})", @event.Id, "admin", @event);
            ConsoleHelper.WriteSuccessLine($"----- Handling integration event: {@event.Id} at admin - ({@event})");
            return Task.FromResult("1");
        }

    }
}
