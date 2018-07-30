using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ServiceBus.Messaging;

namespace NCS.DSS.ContentPushService
{
    public static class WestMidsAndStaffsTopicListener
    {
        [FunctionName("WestMidsAndStaffsTopicListener")]
        public static async System.Threading.Tasks.Task RunAsync(
            [ServiceBusTrigger("westmidsandstaffs", "westmidsandstaffs", AccessRights.Listen, Connection = "ServiceBusConnectionString")]string ServiceBusMessage, 
            TraceWriter log)
        {
            MessagePushService messagePushService = new MessagePushService();
            await messagePushService.PushToTouchpoint(ServiceBusMessage);
        }

    }
}
