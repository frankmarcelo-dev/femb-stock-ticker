using Serilog;
using FembStockTicker.Services;
using FembStockTicker.Middleware;
using FembStockTicker.Swagger;
using FembStockTicker.Config;

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
            // Configure the HTTP request pipeline.
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
            // Configure Swagger
            var appSettings = configuration.Get<AppSettings>()
                ?? throw new InvalidOperationException("AppSettings configuration is missing.");
            builder.Services.AddSingleton(appSettings);

            SwaggerConfiguration.ConfigureSwaggerWith(appSettings.AppConfiguration);

            builder.Services.AddControllers();
            // register any application services
            builder.Services.AddSingleton<IWeatherForecastService, WeatherForecastService>();

            // Swagger/OpenAPI
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c => SwaggerConfiguration.WithSwaggerGenServiceOptions(c));

            // authentication/authorization, caching, etc. would be configured here
            builder.Services.AddAuthentication();
            builder.Services.AddAuthorization();
            builder.Services.AddResponseCaching();
        }
    }
}

