using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ServiceBus.Messaging;
using NCS.DSS.ContentPushService.PushService;

namespace NCS.DSS.ContentPushService.Listeners
{
    public static class CareersHelplineTopicListener
    {
        [FunctionName("CareersHelplineTopicListener")]
        public static async System.Threading.Tasks.Task RunAsync(
            [ServiceBusTrigger("careershelpline", "careershelpline", AccessRights.Listen, Connection = "ServiceBusConnectionString")]string serviceBusMessage, 
            TraceWriter log)
        {
            var messagePushService = new MessagePushService();
            await messagePushService.PushToTouchpoint(serviceBusMessage);
        }

    }
}
