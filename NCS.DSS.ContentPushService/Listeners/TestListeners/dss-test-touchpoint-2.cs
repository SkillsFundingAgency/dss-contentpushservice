using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.ServiceBus.Messaging;
using NCS.DSS.ContentPushService.PushService;

namespace NCS.DSS.ContentPushService.Listeners
{
    public class DSSTestTouchpoint2TopicListener
    {
        private const string TopicName = "dss-test-touchpoint-2";
        private const string SubscriptionName = "dss-test-touchpoint-2";
        private const string AppIdUri = "dss-test-touchpoint-2.AppIdUri";
        private const string ClientUrl = "dss-test-touchpoint-2.Url";

        [FunctionName("dsstesttouchpoint2TopicListener")]
        public async Task RunAsync(
            [ServiceBusTrigger(TopicName, SubscriptionName, Connection = "ServiceBusConnectionString")]BrokeredMessage serviceBusMessage,
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
                log.LogInformation("The " + TopicName + " topic successfully pushed a notification to " + ClientUrl + " at " + DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
                throw;
            }

        }
    }
}
