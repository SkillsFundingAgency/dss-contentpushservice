using System;
using System.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.ServiceBus.Messaging;
using NCS.DSS.ContentPushService.Auth;
using NCS.DSS.ContentPushService.PushService;

namespace NCS.DSS.ContentPushService.Listeners
{
    public static class CareersHelplineTopicListener
    {
        private const string TopicName = "careershelpline";
        private const string SubscriptionName = "careershelpline";
        private const string AppIdUri = "CareersHelpline.AppIdUri";
        private const string ClientUrl = "CareersHelpline.Url";

        [FunctionName("CareersHelplineTopicListener")]
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
                await messagePushService.PushToTouchpoint(AppIdUri, ClientUrl, serviceBusMessage);
            }
            catch (Exception ex)
            {
                log.LogInformation(ex.ToString());
                return;
            }

        }
    }
}
