using Azure.Identity;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
using Microsoft.Extensions.Options;
using NCS.DSS.ContentPushService.Cosmos.Provider;
using NCS.DSS.ContentPushService.Listeners;
using NCS.DSS.ContentPushService.Models;
using NCS.DSS.ContentPushService.PushService;
using NCS.DSS.ContentPushService.Services;

namespace NCS.DSS.ContentPushService
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var host = new HostBuilder()
            .ConfigureFunctionsWorkerDefaults()
            .ConfigureServices((context, services) =>
            {
                var configuration = context.Configuration;
                services.AddOptions<ContentPushServiceConfigurationSettings>()
                    .Bind(configuration);

                services.AddApplicationInsightsTelemetryWorkerService();
                services.ConfigureFunctionsApplicationInsights();
                services.AddLogging(logging =>
                {
                    logging.Services.Configure<LoggerFilterOptions>(options =>
                    {
                        var defaultRule = options.Rules.FirstOrDefault(rule => rule.ProviderName
                            == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");
                        if (defaultRule is not null)
                        {
                            options.Rules.Remove(defaultRule);
                        }

                        logging.AddFilter<ApplicationInsightsLoggerProvider>("Azure.Messaging.ServiceBus", LogLevel.Warning);

                    });
                });

                services.AddTransient<IListenersHelper, ListenersHelper>();
                services.AddTransient<IMessagePushService, MessagePushService>();
                services.AddTransient<IRequeueService, RequeueService>();
                services.AddTransient<ICosmosDBProvider, CosmosDBProvider>();

                services.AddSingleton(s =>
                {
                    var logger = s.GetRequiredService<ILogger<Program>>();

                    var connectionString = configuration["CosmosDBConnectionString"];
                    var endpoint = configuration["CosmosDbEndpoint"];

                    var options = new CosmosClientOptions
                    {
                        ConnectionMode = ConnectionMode.Gateway
                    };

                    if (!string.IsNullOrWhiteSpace(endpoint))
                    {
                        logger.LogInformation("Using DefaultAzureCredential for Cosmos DB (managed identity)");
                        return new CosmosClient(endpoint, new DefaultAzureCredential(), options);
                    }
                    else if (!string.IsNullOrWhiteSpace(connectionString))
                    {
                        logger.LogInformation("No managed identity found: using Cosmos DB connection string (local development)");
                        return new CosmosClient(connectionString, options);
                    }
                    else
                    {
                        throw new InvalidOperationException("Neither CosmosDbEndpoint or a ConnectionString are configured");
                    }
                });               
            })
            .Build();

            await host.RunAsync();
        }
    }
}
