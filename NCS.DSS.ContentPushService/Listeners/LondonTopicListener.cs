using System.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.ServiceBus.Messaging;
using NCS.DSS.ContentPushService.Auth;
using NCS.DSS.ContentPushService.PushService;

namespace NCS.DSS.ContentPushService.Listeners
{
    public static class LondonTopicListener
    {
        private const string TopicName = "london";
        private const string SubscriptionName = "london";

        [FunctionName("LondonTopicListener")]
        public static async System.Threading.Tasks.Task RunAsync(
            [ServiceBusTrigger(TopicName, SubscriptionName, AccessRights.Listen, Connection = "ServiceBusConnectionString")]BrokeredMessage serviceBusMessage, 
             ILogger log)
        {
            if (serviceBusMessage == null)
            {
                log.LogError("Brokered Message cannot be null");
                return;
            }

            var appIdUri = ConfigurationManager.AppSettings["London.AppIdUri"];
            if (string.IsNullOrWhiteSpace(appIdUri))
            {
                log.LogError("unable to find App Id Uri for " + TopicName);
                return;
            }

            var accessToken = await AuthenticationHelper.GetAccessToken(appIdUri);
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                log.LogError("Unable to Generate Token for " + TopicName);
                return;
            }

            var clientUrl = ConfigurationManager.AppSettings["LondonUrl"];
            if (string.IsNullOrWhiteSpace(clientUrl))
            {
                log.LogError("Unable to find Client Url for " + TopicName);
                return;
            }

            var messagePushService = new MessagePushService();
            await messagePushService.PushToTouchpoint(serviceBusMessage, clientUrl, accessToken);
        }
    }
}
