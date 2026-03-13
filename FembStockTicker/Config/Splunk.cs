namespace FembStockTicker.Config
{
    public class Splunk
    {
        public bool Enabled { get; set; }
        public string? LogLevel { get; set; }
        public string? Token { get; set; }
        public string? Host { get; set; }
    }
}