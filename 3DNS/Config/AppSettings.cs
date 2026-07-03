namespace _3DNS.Config
{
    /// <summary>
    /// Strongly typed application configuration bound from appsettings.json.
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// Cloudflare API credentials and managed domains.
        /// </summary>
        public CloudflareSettings Cloudflare { get; set; } = new();

        /// <summary>
        /// PostgreSQL connection string.
        /// </summary>
        public string ConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// API key for the Ting notification endpoint on api.tzer0m.co.uk.
        /// </summary>
        public string TingApiKey { get; set; } = string.Empty;
    }
}