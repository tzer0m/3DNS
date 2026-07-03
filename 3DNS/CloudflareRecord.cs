namespace _3DNS;

/// <summary>
/// A single Cloudflare DNS record.
/// </summary>
public class CloudflareRecord
{
    /// <summary>
    /// The Cloudflare-assigned record ID, required for updates.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The record's value (e.g. the IP address for an A record).
    /// </summary>
    public string Content { get; set; } = string.Empty;
}
