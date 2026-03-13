namespace FembStockTicker.Config
{
    public class AppSettings
    {
        public required AppConfiguration AppConfiguration { get; set; }

        public HealthCheckSettings? HealthCheck { get; set; }

        public required Auth0Configuration Auth0 { get; set; }

        public ApiDirectory? Api { get; set; }

        public CacheSettings? Cache { get; set; }

        public string? NEW_RELIC_APP_NAME { get; set; }

        public string? NEW_RELIC_LICENSE_KEY { get; set; }
    }
}