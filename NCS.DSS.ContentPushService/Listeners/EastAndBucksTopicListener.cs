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
    public static class EastAndBucksTopicListener
    {
        private const string TopicName = "eastandbucks";
        private const string SubscriptionName = "eastandbucks";
        private const string AppIdUri = "EastAndBucks.AppIdUri";
        private const string ClientUrl = "EastAndBucks.Url";

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

            try
            {
                var messagePushService = new MessagePushService();
                await messagePushService.PushToTouchpoint(AppIdUri, ClientUrl, serviceBusMessage, TopicName);

                var body = new StreamReader(serviceBusMessage.GetBody<Stream>(), Encoding.UTF8).ReadToEnd();
                log.LogInformation("The " + TopicName + " topic pushed a notification to " + ClientUrl + " at " + DateTime.Now + " with a payload of " + body);
            }
            catch(Exception ex)
            {
                log.LogError(ex.ToString());
                return;
            }
           
        }
    }
}
