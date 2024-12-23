using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCS.DSS.ContentPushService.Cosmos.Provider;
using NCS.DSS.ContentPushService.Listeners;
using NCS.DSS.ContentPushService.PushService;
using NCS.DSS.ContentPushService.Services;
using NCS.DSS.ContentPushService.Utils;

namespace NCS.DSS.ContentPushService
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = new HostBuilder()
            .ConfigureFunctionsWorkerDefaults()
            .ConfigureServices(services =>
            {
                services.AddApplicationInsightsTelemetryWorkerService();
                services.ConfigureFunctionsApplicationInsights();

                services.AddLogging(log =>
                {
                    log.SetMinimumLevel(LogLevel.Trace);
                });
                services.AddTransient<IListenersHelper, ListenersHelper>();
                services.AddTransient<IMessagePushService, MessagePushService>();
                services.AddTransient<IDigitialIdentityService, DigitialIdentityService>();
                services.AddTransient<IDigitalIdentityClient, DigitalIdentityClient>();
                services.AddTransient<IRequeueService, RequeueService>();
                services.AddTransient<ICosmosDBProvider, CosmosDBProvider>();
                services.AddHttpClient("AzureB2C", client =>
                {
                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Environment.GetEnvironmentVariable("AzureB2C.ApiKey"));
                    client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("AzureB2C.ApiUrl"));
                });

                services.Configure<LoggerFilterOptions>(options =>
                {
                    LoggerFilterRule toRemove = options.Rules.FirstOrDefault(rule => rule.ProviderName
                        == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");
                    if (toRemove is not null)
                    {
                        options.Rules.Remove(toRemove);
                    }
                });

                services.AddSingleton(s =>
                {
                    var cosmosEndpoint = Environment.GetEnvironmentVariable("Endpoint");
                    var cosmosKey = Environment.GetEnvironmentVariable("Key");

                    return new CosmosClient(cosmosEndpoint, cosmosKey);
                });
            })
            .Build();

            await host.RunAsync();
        }
    }
}
