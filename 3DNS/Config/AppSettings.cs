namespace _3DNS.Config
{
    /// <summary>
    /// Strongly typed application configuration bound from appsettings.json.
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// GoDaddy API credentials.
        /// </summary>
        public GoDaddySettings GoDaddy { get; set; } = new();

        /// <summary>
        /// PostgreSQL connection string.
        /// </summary>
        public string ConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// Domains to keep pointed at the current public IP.
        /// </summary>
        public List<string> Domains { get; set; } = [];
    }
}