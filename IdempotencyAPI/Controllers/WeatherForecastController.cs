using IdempotencyAPI.Application.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IdempotencyAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        IMediator _mediator;

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpPost]
        public IEnumerable<WeatherForecast> PostIdempotencyObjectReturn([FromHeader(Name = "Idempotency-Key")] string idempotencyKey, [FromBody]WeatherForecast forecast) 
        {
            IEnumerable<WeatherForecast> weatherForecasts = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
             .ToArray();
            weatherForecasts = weatherForecasts.Append(forecast);

            return weatherForecasts;
        }

        [HttpPost("booleano")]
        public bool PostIdempotencyBooleano([FromHeader(Name = "Idempotency-Key")] string idempotencyKey, [FromBody] WeatherForecast forecast)
        {
            if (forecast.TemperatureC > 0) 
            {
                return true;
            }

            return false;
        }

        [HttpPost("~/booleano")]
        public async Task<bool> PostIdempotencyWithoutMiddlewareBooleano([FromHeader(Name = "Idempotency-Key")] string idempotencyKey, [FromBody] WeatherForecast forecast)
        {
            bool response = await _mediator.Send(new SimulationCommand()
            {
                IdempotencyKey = idempotencyKey,
                WeatherForecast = forecast
            });

            return response;
        }
    }
}
