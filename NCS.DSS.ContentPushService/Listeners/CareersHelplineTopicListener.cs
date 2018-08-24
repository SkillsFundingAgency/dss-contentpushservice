using System.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ServiceBus.Messaging;
using NCS.DSS.ContentPushService.Auth;
using NCS.DSS.ContentPushService.PushService;

namespace NCS.DSS.ContentPushService.Listeners
{
    public static class CareersHelplineTopicListener
    {
        [FunctionName("CareersHelplineTopicListener")]
        public static async System.Threading.Tasks.Task RunAsync(
            [ServiceBusTrigger("careershelpline", "careershelpline", AccessRights.Listen, Connection = "ServiceBusConnectionString")]BrokeredMessage serviceBusMessage, 
            TraceWriter log)
        {

            if (serviceBusMessage == null)
            {
                log.Error("Brokered Message cannot be null");
                return;
            }

            var clientId = ConfigurationManager.AppSettings["CareersHelplineClientId"];
            var clientSecret = ConfigurationManager.AppSettings["CareersHelplineClientSecret"];

            var accessToken = await AuthenticationHelper.GetAccessToken(clientId, clientSecret);

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                log.Warning("Unable to Generate Token");
                return;
            }

            var clientUrl = ConfigurationManager.AppSettings["CareersHelplineUrl"];

            var messagePushService = new MessagePushService();
            await messagePushService.PushToTouchpoint(serviceBusMessage, clientUrl, accessToken);
        }

    }
}
