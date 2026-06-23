namespace _3DNS.Config
{
    /// <summary>
    /// GoDaddy API credentials.
    /// </summary>
    public class GoDaddySettings
    {
        /// <summary>
        /// GoDaddy API key.
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>
        /// GoDaddy API secret.
        /// </summary>
        public string ApiSecret { get; set; } = string.Empty;
    }
}