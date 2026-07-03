using _3DNS;
using _3DNS.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

/// <summary>
/// Entry point
/// </summary>
internal class Program
{
    /// <summary>
    /// Main method
    /// </summary>
    private static void Main()
    {
        // Create logger
        using ILoggerFactory factory = LoggerFactory.Create(builder =>
            builder.AddSimpleConsole(options =>
            {
                options.IncludeScopes = true;
                options.SingleLine = true;
                options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
            }));
        ILogger logger = factory.CreateLogger("3DNS");

        // Load configuration
        IConfigurationRoot configuration = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory).AddJsonFile("appsettings.json", optional: false).Build();
        AppSettings settings = new();
        configuration.Bind(settings);

        if (string.IsNullOrEmpty(settings.Cloudflare.ApiToken) || string.IsNullOrEmpty(settings.ConnectionString) || string.IsNullOrEmpty(settings.TingApiKey) || settings.Cloudflare.Domains.Count == 0)
        {
            logger.LogError("Missing required configuration.");
            return;
        }

        // Get current public IP address (once, shared across all domains)
        logger.LogInformation("Getting current public IP address");
        using HttpClient ipClient = new();
        HttpResponseMessage ipResponse = ipClient.Send(new(HttpMethod.Get, "https://api.ipify.org"));
        try
        {
            ipResponse.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to get public IP");
            return;
        }

        string ip = ipResponse.Content.ReadAsStringAsync().Result;
        if (!System.Net.IPAddress.TryParse(ip, out _))
        {
            logger.LogError("Invalid IP address retrieved: {ip}", ip);
            return;
        }
        logger.LogInformation("Current public IP address: {ip}", ip);

        // Run dynamic DNS update for each configured domain
        Dictionary<string, Outcome> results = [];
        foreach (CloudflareDomainSettings domain in settings.Cloudflare.Domains)
        {
            results[domain.Domain] = DynDNS.Run(logger, domain.Domain, ip, domain.ZoneId, settings.Cloudflare.ApiToken);
        }

        // Write a single database record if any domain actually changed
        if (results.Values.Any(o => o == Outcome.SuccessWithChange))
        {
            WriteIpChange(logger, settings.ConnectionString, ip);
        }

        // Send notification if any domain changed or failed
        if (results.Values.Any(o => o != Outcome.SuccessNoChange))
        {
            string body = string.Join("\n", results.Select(r => $"{r.Key}: {DescribeOutcome(r.Value)}"));
            TingClient.Send(logger, settings.TingApiKey, "3DNS Alert", body);
        }
    }

    /// <summary>
    /// Writes a single IP change record to the database
    /// </summary>
    /// <param name="logger">Logger</param>
    /// <param name="connectionString">Database connection string</param>
    /// <param name="ip">New IP address</param>
    private static void WriteIpChange(ILogger logger, string connectionString, string ip)
    {
        logger.LogInformation("Writing DNS update to database");
        try
        {
            using NpgsqlConnection connection = new(connectionString);
            connection.Open();
            using NpgsqlCommand command = new("INSERT INTO \"DnsUpdates\" (\"IpAddress\", \"RecordedAt\") VALUES (@ip, @recordedAt)", connection);
            command.Parameters.AddWithValue("ip", ip);
            command.Parameters.AddWithValue("recordedAt", DateTime.UtcNow);
            command.ExecuteNonQuery();
            logger.LogInformation("DNS update written to database");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to write DNS update to database");
        }
    }

    /// <summary>
    /// Describes an outcome in human-readable form for notifications
    /// </summary>
    /// <param name="outcome">Outcome</param>
    /// <returns>Description</returns>
    private static string DescribeOutcome(Outcome outcome) => outcome switch
    {
        Outcome.Failure => "Failed to Update",
        Outcome.SuccessWithChange => "IP Changed",
        Outcome.SuccessNoChange => "No Change",
        _ => "Unknown"
    };
}