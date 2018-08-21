using System.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ServiceBus.Messaging;
using NCS.DSS.ContentPushService.Auth;
using NCS.DSS.ContentPushService.PushService;

namespace NCS.DSS.ContentPushService.Listeners
{
    public static class LondonTopicListener
    {
        [FunctionName("LondonTopicListener")]
        public static async System.Threading.Tasks.Task RunAsync(
            [ServiceBusTrigger("london", "london", AccessRights.Listen, Connection = "ServiceBusConnectionString")]string serviceBusMessage, 
            TraceWriter log)
        {
            var clientId = ConfigurationManager.AppSettings["LondonClientId"];
            var clientSecret = ConfigurationManager.AppSettings["LondonClientSecret"];

            var accessToken = await AuthenticationHelper.GetAccessToken(clientId, clientSecret);

            if (accessToken == null)
                return;

            var clientUrl = ConfigurationManager.AppSettings["LondonUrl"];

            var messagePushService = new MessagePushService();
            await messagePushService.PushToTouchpoint(serviceBusMessage, clientUrl, accessToken.AccessToken);
        }

    }
}
