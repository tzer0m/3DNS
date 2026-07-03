using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace _3DNS
{
    /// <summary>
    /// Dyanamic DNS updater for GoDaddy domains
    /// </summary>
    internal class DynDNS
    {
        /// <summary>
        /// Update A record for specified domain to current public IP address if it has changed
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="domain">Domain</param>
        /// <param name="ip">Current public IP address</param>
        /// <param name="zoneId">Cloudflare zone ID for the domain</param>
        /// <param name="apiToken">Cloudflare API token, scoped to DNS edit on the zone</param>
        /// <returns>Outcome of the update</returns>
        public static Outcome Run(ILogger logger, string domain, string ip, string zoneId, string apiToken)
        {
            // Create HTTP client and request
            logger.LogInformation("Getting A record for {domain}", domain);
            HttpClient cfGetClient = new();
            HttpRequestMessage cfGetRequest = new(HttpMethod.Get, $"https://api.cloudflare.com/client/v4/zones/{zoneId}/dns_records?type=A&name={domain}");
            cfGetRequest.Headers.Add("Authorization", $"Bearer {apiToken}");

            // Send request and check for success
            HttpResponseMessage cfGetResponse = cfGetClient.Send(cfGetRequest);
            try
            {
                cfGetResponse.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "Failed to get A record for {domain}", domain);
                return Outcome.Failure;
            }
            logger.LogInformation("Successfully got A record for {domain}", domain);

            // Read response content
            CloudflareListResponse? listResponse = JsonConvert.DeserializeObject<CloudflareListResponse>(cfGetResponse.Content.ReadAsStringAsync().Result);
            if (listResponse is null || !listResponse.Success)
            {
                logger.LogError("Failed to parse A record response for {domain}", domain);
                return Outcome.Failure;
            }
            if (listResponse.Result.Count != 1)
            {
                logger.LogError("Unexpected number of A records for {domain} (expected 1): {count}", domain, listResponse.Result.Count);
                return Outcome.Failure;
            }

            // Check if record already has correct IP
            CloudflareRecord record = listResponse.Result[0];
            if (record.Content == ip)
            {
                logger.LogInformation("A record for {domain} is already up to date", domain);
                return Outcome.SuccessNoChange;
            }
            logger.LogWarning("IP address for {domain} has changed from {oldIp} to {newIp}", domain, record.Content, ip);

            // Create HTTP client and request
            logger.LogInformation("Updating A record for {domain} to {ip}", domain, ip);
            HttpClient cfPatchClient = new();
            HttpRequestMessage cfPatchRequest = new(HttpMethod.Patch, $"https://api.cloudflare.com/client/v4/zones/{zoneId}/dns_records/{record.Id}");
            cfPatchRequest.Headers.Add("Authorization", $"Bearer {apiToken}");
            cfPatchRequest.Content = new StringContent(JsonConvert.SerializeObject(new { content = ip }), null, "application/json");

            // Send request and check for success
            HttpResponseMessage cfPatchResponse = cfPatchClient.Send(cfPatchRequest);
            try
            {
                cfPatchResponse.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "Failed to update A record");
                return Outcome.Failure;
            }

            // Return success
            logger.LogInformation("Successfully updated A record");
            return Outcome.SuccessWithChange;
        }
    }
}