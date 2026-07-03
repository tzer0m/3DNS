namespace _3DNS.Config
{
    /// <summary>
    /// Cloudflare API credentials and the domains to keep pointed at the current public IP.
    /// </summary>
    public class CloudflareSettings
    {
        /// <summary>
        /// API token scoped to DNS edit permission on the zones below.
        /// </summary>
        public string ApiToken { get; set; } = string.Empty;

        /// <summary>
        /// Domains to keep pointed at the current public IP, each paired with its Cloudflare zone ID.
        /// </summary>
        public List<CloudflareDomainSettings> Domains { get; set; } = [];
    }
}