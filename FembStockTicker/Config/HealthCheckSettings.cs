namespace FembStockTicker.Config
{
    public class HealthCheckSettings
    {
        public virtual string StatusClientKey { get; set; } = string.Empty;

        public virtual string StatusKeyHeaderName { get; set; } = string.Empty;

        public static implicit operator HealthCheckSettings(AppSettings settings)
        {
            return settings.HealthCheck ?? throw new ArgumentException(nameof(HealthCheckSettings));
        }
    }
}
