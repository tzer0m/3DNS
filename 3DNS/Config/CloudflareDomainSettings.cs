namespace _3DNS.Config
{
    /// <summary>
    /// A single domain managed by 3DNS, paired with its Cloudflare zone ID.
    /// </summary>
    public class CloudflareDomainSettings
    {
        /// <summary>
        /// The domain name (e.g. tzer0m.co.uk).
        /// </summary>
        public string Domain { get; set; } = string.Empty;

        /// <summary>
        /// The Cloudflare-assigned zone ID for this domain.
        /// </summary>
        public string ZoneId { get; set; } = string.Empty;
    }
}