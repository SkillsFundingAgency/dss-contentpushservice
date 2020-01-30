using System;
using System.Configuration;
using System.IO;
using System.Text;
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
        private const string AppIdUri = "London.AppIdUri";
        private const string ClientUrl = "London.Url";

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

            try
            {
                var messagePushService = new MessagePushService();
                await messagePushService.PushToTouchpoint(AppIdUri, ClientUrl, serviceBusMessage, TopicName, log);
                log.LogInformation("The " + TopicName + " topic successfully pushed a notification to " + ClientUrl + " at " + DateTime.Now);
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
                throw;
            }

        }
    }
}
