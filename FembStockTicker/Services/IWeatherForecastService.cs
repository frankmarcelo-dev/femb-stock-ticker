using FembStockTicker.Models;

namespace FembStockTicker.Services
{
    public interface IWeatherForecastService
    {
        WeatherForecast[] GetForecasts(int days);
    }
}