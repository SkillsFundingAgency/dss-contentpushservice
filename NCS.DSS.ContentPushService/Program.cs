using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NCS.DSS.ContentPushService.Listeners;
using NCS.DSS.ContentPushService.PushService;
using NCS.DSS.ContentPushService.Services;
using NCS.DSS.ContentPushService.Utils;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddLogging(log =>
        {
            log.SetMinimumLevel(LogLevel.Trace);
        });
        services.AddTransient<IListenersHelper, ListenersHelper>();
        services.AddTransient<IMessagePushService, MessagePushService>();
        services.AddTransient<IDigitialIdentityService, DigitialIdentityService>();
        services.AddTransient<IDigitalIdentityClient, DigitalIdentityClient>();
        services.AddTransient<IRequeueService, RequeueService>();
        services.AddTransient<IMessageReceiverService, MessageReceiverService>();
        services.AddHttpClient("AzureB2C", client =>
        {
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Environment.GetEnvironmentVariable("AzureB2C.ApiKey"));
            client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("AzureB2C.ApiUrl"));
        });
    })
    .Build();

host.Run();
