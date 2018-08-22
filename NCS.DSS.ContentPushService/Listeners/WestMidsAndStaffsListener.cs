using System.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ServiceBus.Messaging;
using NCS.DSS.ContentPushService.Auth;
using NCS.DSS.ContentPushService.PushService;

namespace NCS.DSS.ContentPushService.Listeners
{
    public static class WestMidsAndStaffsTopicListener
    {
        [FunctionName("WestMidsAndStaffsTopicListener")]
        public static async System.Threading.Tasks.Task RunAsync(
            [ServiceBusTrigger("westmidsandstaffs", "westmidsandstaffs", AccessRights.Listen, Connection = "ServiceBusConnectionString")]string serviceBusMessage, 
            TraceWriter log)
        {
            var clientId = ConfigurationManager.AppSettings["WestMidsAndStaffsClientId"];
            var clientSecret = ConfigurationManager.AppSettings["WestMidsAndStaffsClientSecret"];

            var accessToken = await AuthenticationHelper.GetAccessToken(clientId , clientSecret);

            if (string.IsNullOrWhiteSpace(accessToken))
                return;

            var clientUrl = ConfigurationManager.AppSettings["WestMidsAndStaffsUrl"];

            var messagePushService = new MessagePushService();
            await messagePushService.PushToTouchpoint(serviceBusMessage, clientUrl, accessToken);
        }

    }
}
