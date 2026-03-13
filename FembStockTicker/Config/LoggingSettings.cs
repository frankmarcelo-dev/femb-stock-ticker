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
    }
}
