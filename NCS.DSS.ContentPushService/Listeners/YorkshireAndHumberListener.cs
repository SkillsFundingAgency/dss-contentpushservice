using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ServiceBus.Messaging;

namespace NCS.DSS.ContentPushService
{
    public static class YorkshireAndHumberTopicListener
    {
        [FunctionName("YorkshireAndHumberTopicListener")]
        public static async System.Threading.Tasks.Task RunAsync(
            [ServiceBusTrigger("yorkshireandhumber", "yorkshireandhumber", AccessRights.Listen, Connection = "ServiceBusConnectionString")]string ServiceBusMessage, 
            TraceWriter log)
        {
            MessagePushService messagePushService = new MessagePushService();
            await messagePushService.PushToTouchpoint(ServiceBusMessage);
        }

    }
}
