namespace FembStockTicker.Config
{
    public class Auth0Configuration
    {
        public required string Domain { get; set; }
        public required string Audience { get; set; }
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }
        public string? TokenEndpoint => $"https://{Domain}/oauth/token";
        public string Authority => $"https://{Domain}/";
        public IList<string> Scopes { get; set; } = ["openid", "profile", "email"];
    }
}