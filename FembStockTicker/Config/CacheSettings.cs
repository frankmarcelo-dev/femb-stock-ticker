
namespace FembStockTicker.Config
{
    public class CacheSettings
    {
        public CacheApplicationConfiguration ApplicationConfiguration { get; set; } = new();

        public static implicit operator CacheSettings(AppSettings settings)
        {
            return settings.Cache ?? throw new ArgumentException(nameof(CacheSettings));
        }
    }
}
