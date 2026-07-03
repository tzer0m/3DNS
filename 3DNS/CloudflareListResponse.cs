namespace _3DNS;

/// <summary>
/// Wrapper for Cloudflare's list DNS records API response.
/// </summary>
public class CloudflareListResponse
{
    /// <summary>
    /// Whether the API call succeeded.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// The list of matching DNS records.
    /// </summary>
    public List<CloudflareRecord> Result { get; set; } = [];
}