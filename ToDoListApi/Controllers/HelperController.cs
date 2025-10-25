using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ToDoListCore;

namespace ToDoListApi.Controllers
{
    // [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class HelperController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<HelperController> _logger;

        public HelperController(ILogger<HelperController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Endpoint for testing. Returns "Test"
        /// </summary>
        /// <returns></returns>
        [HttpGet("uncertainty/test")]
        [ProducesResponseType(200, Type = typeof(string))]
        public string Test()
        {
            return "Test";
        }

        /// <summary>
        /// Simple API test
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetWeatherForecast")]
        public IList<WeatherForecast> GetWeatherForecast()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                //Date = DateTime.Now.AddDays(index),
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToList();
        }

        /// <summary>
        /// Get authorisation server (Identity Server) url. 
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetAuthServerUrl")]
        public string GetAuthServerUrl()
        {
            var authServerUrl = CoreUtilities.GetCurrentProjectUrl(Configuration.AppSettings).TrimEnd('/');
            return authServerUrl;
        }

    }
}
