using System.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.ServiceBus.Messaging;
using NCS.DSS.ContentPushService.Auth;
using NCS.DSS.ContentPushService.PushService;

namespace NCS.DSS.ContentPushService.Listeners
{
    public static class EastAndBucksTopicListener
    {
        private const string TopicName = "eastandbucks";
        private const string SubscriptionName = "eastandbucks";

        [FunctionName("EastAndBucksTopicListener")]
        public static async System.Threading.Tasks.Task RunAsync(
            [ServiceBusTrigger(TopicName, SubscriptionName, AccessRights.Listen, Connection = "ServiceBusConnectionString")]BrokeredMessage serviceBusMessage, 
             ILogger log)
        {
            if (serviceBusMessage == null)
            {
                log.LogError("Brokered Message cannot be null");
                return;
            }

            var clientId = ConfigurationManager.AppSettings["EastAndBucksClientId"];
            if (string.IsNullOrWhiteSpace(clientId))
            {
                log.LogError("unable to find client Id for " + TopicName);
                return;
            }

            var clientSecret = ConfigurationManager.AppSettings["EastAndBucksClientSecret"];
            if (string.IsNullOrWhiteSpace(clientId))
            {
                log.LogError("unable to find client secret for " + TopicName);
                return;
            }

            var accessToken = await AuthenticationHelper.GetAccessToken(clientId, clientSecret);
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                log.LogError("Unable to Generate Token for " + TopicName);
                return;
            }

            var clientUrl = ConfigurationManager.AppSettings["EastAndBucksUrl"];
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
