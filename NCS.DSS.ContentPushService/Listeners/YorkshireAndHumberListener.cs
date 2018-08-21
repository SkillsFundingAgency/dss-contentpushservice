using System.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ServiceBus.Messaging;
using NCS.DSS.ContentPushService.Auth;
using NCS.DSS.ContentPushService.PushService;

namespace NCS.DSS.ContentPushService.Listeners
{
    public static class YorkshireAndHumberTopicListener
    {
        [FunctionName("YorkshireAndHumberTopicListener")]
        public static async System.Threading.Tasks.Task RunAsync(
            [ServiceBusTrigger("yorkshireandhumber", "yorkshireandhumber", AccessRights.Listen, Connection = "ServiceBusConnectionString")]string serviceBusMessage, 
            TraceWriter log)
        {
            var clientId = ConfigurationManager.AppSettings["YorkshireAndHumberClientId"];
            var clientSecret = ConfigurationManager.AppSettings["YorkshireAndHumberClientSecret"];

            var accessToken = await AuthenticationHelper.GetAccessToken(clientId, clientSecret);

            if (accessToken == null)
                return;

            var clientUrl = ConfigurationManager.AppSettings["YorkshireAndHumberUrl"];

            var messagePushService = new MessagePushService();
            await messagePushService.PushToTouchpoint(serviceBusMessage, clientUrl, accessToken.AccessToken);
        }

    }
}
