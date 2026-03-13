using System.Diagnostics.CodeAnalysis;
using FembStockTicker.Config;

namespace FembStockTicker.Swagger
{
    [ExcludeFromCodeCoverage]
    public class SwaggerConfigurationSettings
    {
        private const string DefaultEnvironmentName = "Unknown Environment";
        public string? ApiName { get; set; }
        public string? ApiVersion { get; set; }
        public string? ApiDescription { get; set; }
        public string? ApiHost { get; set; }

        public static implicit operator SwaggerConfigurationSettings(AppConfiguration appConfiguration)
        {
            SwaggerConfigurationSettings options = new SwaggerConfigurationSettings
            {
                ApiDescription = appConfiguration.ApiDescription,
                ApiVersion = appConfiguration.ApiVersion,
                ApiName = GetEnvironmentNameFrom(appConfiguration),
                ApiHost = appConfiguration.ApplicationHost
            };
            return options;
        }

        private static string GetEnvironmentNameFrom(AppConfiguration appConfiguration)
        {
            var applicationName = appConfiguration.ApplicationName;
            var environmentName = appConfiguration.Environment ?? DefaultEnvironmentName;
            return $"{applicationName} - {environmentName}".Trim();
        }
    }
}
