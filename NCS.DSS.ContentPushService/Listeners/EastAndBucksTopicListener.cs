using System;
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

            try
            {
                string appIdUri = ConfigurationManager.AppSettings["EastAndBucks.AppIdUri"].ToString();

                var accessToken = await AuthenticationHelper.GetAccessToken(appIdUri);

                string clientUrl = ConfigurationManager.AppSettings["EastAndBucksUrl"].ToString();

                var messagePushService = new MessagePushService();

                await messagePushService.PushToTouchpoint(serviceBusMessage, clientUrl, accessToken);

            }
            catch(Exception ex)
            {
                log.LogInformation(ex.ToString());
                return;
            }
           
        }
    }
}
