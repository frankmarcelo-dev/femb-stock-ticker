namespace FembStockTicker.Config
{
    public class ApiDirectory
    {
        public static implicit operator ApiDirectory(AppSettings settings)
        {
          return settings.Api ?? throw new ArgumentException(nameof(ApiDirectory));
        }
  }
}
