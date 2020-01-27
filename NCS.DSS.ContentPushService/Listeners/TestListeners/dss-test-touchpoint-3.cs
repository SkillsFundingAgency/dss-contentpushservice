using System;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using NCS.DSS.ContentPushService.PushService;

namespace NCS.DSS.ContentPushService.Listeners
{
    public class DSSTestTouchpoint3TopicListener
    {
        private const string TopicName = "dss-test-touchpoint-3";
        private const string SubscriptionName = "dss-test-touchpoint-3";
        private const string AppIdUri = "dss-test-touchpoint-3.AppIdUri";
        private const string ClientUrl = "dss-test-touchpoint-3.Url";

        [FunctionName("dsstesttouchpoint3TopicListener")]
        public async Task RunAsync(
            [ServiceBusTrigger(TopicName, SubscriptionName, Connection = "ServiceBusConnectionString")]Message serviceBusMessage,
              MessageReceiver messageReceiver, ILogger log)
        {
            if (serviceBusMessage == null)
            {
                log.LogError("Brokered Message cannot be null");
                return;
            }

            try
            {
                var messagePushService = new MessagePushService();
                await messagePushService.PushToTouchpoint(AppIdUri, ClientUrl, serviceBusMessage, TopicName, messageReceiver, log);
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
