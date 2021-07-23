using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Common.Helper;

namespace EvenBus.Extensions.EventHandling
{
    public class DeletedIntegrationEventHandlerDir : IDynamicIntegrationEventHandler
    {
        private readonly ILogger<DeletedIntegrationEventHandlerDir> _logger;

        public DeletedIntegrationEventHandlerDir(
            ILogger<DeletedIntegrationEventHandlerDir> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        //public Task Handle(DeletedIntegrationEvent @event)
        //{
        //    _logger.LogInformation("----- Handling integration event: {IntegrationEventId} at {AppName} - ({@IntegrationEvent})", @event.Id, "admin", @event);
        //    ConsoleHelper.WriteSuccessLine($"----- Handling integration event: {@event.Id} at admin - ({@event})");
        //    return Task.FromResult("1");
        //}

        public Task Handle(dynamic eventData)
        {
            _logger.LogInformation("12312312");
            ConsoleHelper.WriteSuccessLine($"----- Handling integration event: {eventData.Id} at admin - ({eventData})");
            return Task.FromResult("1");
        }
    }
}
