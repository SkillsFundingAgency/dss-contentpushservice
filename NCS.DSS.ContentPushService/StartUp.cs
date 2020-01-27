using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using NCS.DSS.ContentPushService;
using NCS.DSS.ContentPushService.Listeners;
using NCS.DSS.ContentPushService.PushService;

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
        }
    }
}
