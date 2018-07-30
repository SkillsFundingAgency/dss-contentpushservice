using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ServiceBus.Messaging;

namespace NCS.DSS.ContentPushService
{
    public static class SouthEastTopicListener
    {
        [FunctionName("SouthEastTopicListener")]
        public static async System.Threading.Tasks.Task RunAsync(
            [ServiceBusTrigger("southeast", "southeast", AccessRights.Listen, Connection = "ServiceBusConnectionString")]string ServiceBusMessage, 
            TraceWriter log)
        {
            MessagePushService messagePushService = new MessagePushService();
            await messagePushService.PushToTouchpoint(ServiceBusMessage);
        }

    }
}
