using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCS.DSS.ContentPushService.Cosmos.Provider;
using NCS.DSS.ContentPushService.Listeners;
using NCS.DSS.ContentPushService.Models;
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
            .ConfigureServices((context, services) =>
            {
                var configuration = context.Configuration;
                services.AddOptions<ContentPushServiceConfigurationSettings>()
                    .Bind(configuration);

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
                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", configuration["AzureB2CApiKey"]);
                    client.BaseAddress = new Uri(configuration["AzureB2CApiUrl"]);
                });

                services.AddSingleton(s =>
                {
                    var settings = s.GetRequiredService<IOptions<ContentPushServiceConfigurationSettings>>().Value;
                    var options = new CosmosClientOptions() { ConnectionMode = ConnectionMode.Gateway };

                    return new CosmosClient(settings.Endpoint, settings.Key);
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
            })
            .Build();

            await host.RunAsync();
        }
    }
}
