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
    public static class WestMidsAndStaffsTopicListener
    {
        private const string TopicName = "westmidsandstaffs";
        private const string SubscriptionName = "westmidsandstaffs";
        private const string AppIdUri = "WestMidsAndStaffs.AppIdUri";
        private const string ClientUrl = "WestMidsAndStaffs.Url";

        [FunctionName("WestMidsAndStaffsTopicListener")]
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
