using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using EvenBus.Extensions;
using EvenBus.Extensions.EventHandling;
using Microsoft.AspNetCore.Authorization;

namespace Evenbus.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            Random rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
        [HttpGet]
        [AllowAnonymous]
        [Route("PublishTry")]
        public void PublishTry([FromServices] IEventBus _eventBus, string id = "1")
        {
            DeletedIntegrationEvent deletedEvent = new DeletedIntegrationEvent(id);

            _eventBus.Publish(deletedEvent);
        }



        [HttpGet]
        [AllowAnonymous]
        [Route("PublishTry2")]

        public void PublishTry2([FromServices] IEventBus _eventBus, string id = "1")
        {
            SelectIntegrationEvent selectEvent = new SelectIntegrationEvent(id);
            _eventBus.Publish(selectEvent);
        }
    }
}
