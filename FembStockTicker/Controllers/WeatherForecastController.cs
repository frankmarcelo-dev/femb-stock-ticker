using Microsoft.AspNetCore.Mvc;
using FembStockTicker.Models;
using FembStockTicker.Services;

namespace FembStockTicker.Controllers
{
    [ApiController]
    [Route("api/weatherforecast")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly IWeatherForecastService _forecastService;

        public WeatherForecastController(IWeatherForecastService forecastService)
        {
            _forecastService = forecastService;
        }

        [HttpGet]
        public ActionResult<WeatherForecast[]> Get(int days = 5)
        {
            var result = _forecastService.GetForecasts(days);
            return Ok(result);
        }
    }
}