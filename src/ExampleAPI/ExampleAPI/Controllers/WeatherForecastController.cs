using Microsoft.AspNetCore.Mvc;

namespace ExampleAPI.Controllers
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

        [HttpGet(Name = "GetWeatherForecast")]
        public IActionResult Get(string? numDays = "5")
        {
            // For response codes that are uncommon, there may not be named methods in which case you can use StatusCode().
            //return StatusCode(418, "Stop trying to brew coffee with a teapot!");

            int numDaysInt;
            if (!int.TryParse(numDays, out numDaysInt)) return BadRequest(new Exception("numDays must be a positive integer."));
            else
            {
                return Ok(Enumerable.Range(1, numDaysInt).Select(index => new WeatherForecast
                {
                    Date = DateTime.Now.AddDays(index),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                })
                .ToArray());
            }
            
        }
    }
}