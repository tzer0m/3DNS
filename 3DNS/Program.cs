using _3DNS;
using _3DNS.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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

        if (string.IsNullOrEmpty(settings.GoDaddy.ApiKey) || string.IsNullOrEmpty(settings.GoDaddy.ApiSecret) || string.IsNullOrEmpty(settings.ConnectionString) || settings.Domains.Count == 0)
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
        foreach (string domain in settings.Domains)
        {
            DynDNS.Run(logger, domain, ip, settings.GoDaddy.ApiKey, settings.GoDaddy.ApiSecret, settings.ConnectionString);
        }
    }
}