using _3DNS;
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
        string? domain = ConfigHelper.GetValue(logger, "Domain");
        string? apiKey = ConfigHelper.GetValue(logger, "ApiKey");
        string? apiSecret = ConfigHelper.GetValue(logger, "ApiSecret");
        string? connectionString = ConfigHelper.GetValue(logger, "ConnectionString");
        if (domain is null || apiKey is null || apiSecret is null || connectionString is null)
        {
            logger.LogError("Missing required configuration.");
            return;
        }

        // Run dynamic DNS update
        DynDNS.Run(logger, domain, apiKey, apiSecret, connectionString);
    }
}