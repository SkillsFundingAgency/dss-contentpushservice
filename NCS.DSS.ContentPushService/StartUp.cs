using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using NCS.DSS.ContentPushService;
using NCS.DSS.ContentPushService.Listeners;
using NCS.DSS.ContentPushService.PushService;
using NCS.DSS.ContentPushService.Services;
using NCS.DSS.ContentPushService.Utils;
using System;
using System.Net.Http.Headers;

[assembly: FunctionsStartup(typeof(Startup))]

namespace NCS.DSS.ContentPushService
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            ConfigureServices(builder.Services); 
        }

        private void ConfigureServices(IServiceCollection services)
        {
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
        }
    }
}
