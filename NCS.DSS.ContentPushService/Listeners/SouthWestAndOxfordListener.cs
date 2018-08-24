using System.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ServiceBus.Messaging;
using NCS.DSS.ContentPushService.Auth;
using NCS.DSS.ContentPushService.PushService;

namespace NCS.DSS.ContentPushService.Listeners
{
    public static class SouthWestAndOxfordTopicListener
    {
        [FunctionName("SouthWestAndOxfordTopicListener")]
        public static async System.Threading.Tasks.Task RunAsync(
            [ServiceBusTrigger("southwestandoxford", "southwestandoxford", AccessRights.Listen, Connection = "ServiceBusConnectionString")]BrokeredMessage serviceBusMessage, 
            TraceWriter log)
        {
            if (serviceBusMessage == null)
            {
                log.Error("Brokered Message cannot be null");
                return;
            }

            var clientId = ConfigurationManager.AppSettings["SouthWestAndOxfordClientId"];
            var clientSecret = ConfigurationManager.AppSettings["SouthWestAndOxfordClientSecret"];

            var accessToken = await AuthenticationHelper.GetAccessToken(clientId, clientSecret);

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                log.Warning("Unable to Generate Token");
                return;
            }

            var clientUrl = ConfigurationManager.AppSettings["SouthWestAndOxfordUrl"];

            var messagePushService = new MessagePushService();
            await messagePushService.PushToTouchpoint(serviceBusMessage, clientUrl, accessToken);
        }

    }
}
