using AirAware.Weather.Services;
using Microsoft.AspNetCore.Mvc;

namespace AirAware.Weather.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WeatherTestController : ControllerBase
    {
        private readonly WeatherLogicService _service;

        public WeatherTestController(WeatherLogicService service)
        {
            _service = service;
        }

        [HttpGet("analyze")]
        public async Task<IActionResult> Analyze(string airportCode = "DXB", double hours = 9, string locationName = "Dubai International Airport")
        {
            var report = await _service.AnalyzeConditionsAsync(airportCode, hours, locationName);
            return Ok(report);
        }
    }
}