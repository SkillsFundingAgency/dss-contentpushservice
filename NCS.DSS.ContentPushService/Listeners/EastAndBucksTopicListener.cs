using System.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ServiceBus.Messaging;
using NCS.DSS.ContentPushService.Auth;
using NCS.DSS.ContentPushService.PushService;

namespace NCS.DSS.ContentPushService.Listeners
{
    public static class EastAndBucksTopicListener
    {
        [FunctionName("EastAndBucksTopicListener")]
        public static async System.Threading.Tasks.Task RunAsync(
            [ServiceBusTrigger("eastandbucks", "eastandbucks", AccessRights.Listen, Connection = "ServiceBusConnectionString")]BrokeredMessage serviceBusMessage, 
            TraceWriter log)
        {
            if (serviceBusMessage == null)
            {
                log.Error("Brokered Message cannot be null");
                return;
            }

            var clientId = ConfigurationManager.AppSettings["EastAndBucksClientId"];
            var clientSecret = ConfigurationManager.AppSettings["EastAndBucksClientSecret"];

            var accessToken = await AuthenticationHelper.GetAccessToken(clientId, clientSecret);

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                log.Warning("Unable to Generate Token");
                return;
            }

            var clientUrl = ConfigurationManager.AppSettings["EastAndBucksUrl"];

            var messagePushService = new MessagePushService();
            await messagePushService.PushToTouchpoint(serviceBusMessage, clientUrl, accessToken);
        }

    }
}
