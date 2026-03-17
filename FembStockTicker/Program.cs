using Serilog;
using FembStockTicker.Services;
using FembStockTicker.Middleware;
using FembStockTicker.Swagger;
using FembStockTicker.Config;
using FembStockTicker.Auth0;

namespace FembStockTicker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure Serilog
            builder.Host.UseSerilog((context, configuration) =>
                configuration.ReadFrom.Configuration(context.Configuration));

            // Add services to the container.
            ConfigureServices(builder, builder.Configuration);

            var app = builder.Build();
            app.UseMiddleware<CorrelationIdHeaderMiddleware>();
            app.UseSerilogRequestLogging();
            app.UseMiddleware<ExceptionHandlerMiddleware>();
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseResponseCaching();
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger(SwaggerConfiguration.WithSwaggerOptions);
                app.UseSwaggerUI(SwaggerConfiguration.WithSwaggerUiOptions);
            }

            app.MapControllers();
            app.Run();
        }

        private static void ConfigureServices(WebApplicationBuilder builder, ConfigurationManager configuration)
        {
            var appSettings = configuration.Get<AppSettings>()
                ?? throw new InvalidOperationException("AppSettings configuration is missing.");
            builder.Services.AddSingleton(appSettings);

            // Configure Swagger
            SwaggerConfiguration.ConfigureSwaggerWith(appSettings.AppConfiguration);
            builder.Services.AddControllers();
            builder.Services.AddSingleton<IWeatherForecastService, WeatherForecastService>();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c => SwaggerConfiguration.WithSwaggerGenServiceOptions(c));

            // Auth0
            builder.Services.AddAuth0Authentication(appSettings.Auth0);
            builder.Services.AddAuth0Authorization(appSettings.Auth0);
            builder.Services.AddResponseCaching();
        }
    }
}
