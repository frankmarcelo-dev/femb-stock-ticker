namespace FembStockTicker.Config
{
    public class LoggingSettings
    {
        public bool IncludeScopes { get; set; }
        public string? LogOutputTemplate { get; set; }
        public Trace? Trace { get; set; }
        public Splunk? Splunk { get; set; }
        public string? LogLevel { get; set; }
        public string? ExcludeSourceFilters { get; set; }

        public static implicit operator LoggingSettings(AppSettings settings)
        {
            throw new NotImplementedException();
        }
    }

    public class Trace
    {
        public bool Enabled { get; set; }
        public string? LogLevel { get; set; }
    }

    public class Splunk
    {
        public bool Enabled { get; set; }
        public string? LogLevel { get; set; }
        public string? Token { get; set; }
        public string? Host { get; set; }
    }
}
