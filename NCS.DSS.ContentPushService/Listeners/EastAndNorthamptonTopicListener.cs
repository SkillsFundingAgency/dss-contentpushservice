using System.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ServiceBus.Messaging;
using NCS.DSS.ContentPushService.Auth;
using NCS.DSS.ContentPushService.PushService;

namespace NCS.DSS.ContentPushService.Listeners
{
    public static class EastAndNorthamptonTopicListener
    {
        [FunctionName("EastAndNorthamptonTopicListener")]
        public static async System.Threading.Tasks.Task RunAsync(
            [ServiceBusTrigger("eastandnorthampton", "eastandnorthampton", AccessRights.Manage, Connection = "ServiceBusConnectionString")]string serviceBusMessage,
            TraceWriter log)
        {
            var clientId = ConfigurationManager.AppSettings["EastAndNorthamptonClientId"];
            var clientSecret = ConfigurationManager.AppSettings["EastAndNorthamptonClientSecret"];

            var accessToken = await AuthenticationHelper.GetAccessToken(clientId, clientSecret);

            if (string.IsNullOrWhiteSpace(accessToken))
                return;

            var clientUrl = ConfigurationManager.AppSettings["EastAndNorthamptonUrl"];

            var messagePushService = new MessagePushService();
            await messagePushService.PushToTouchpoint(serviceBusMessage, clientUrl, accessToken);
        }

    }
}

