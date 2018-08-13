using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ServiceBus.Messaging;
using NCS.DSS.ContentPushService.PushService;

namespace NCS.DSS.ContentPushService.Listeners
{
    public static class NorthEastAndCumbriaListener
    {
        [FunctionName("NorthEastAndCumbriaListener")]
        public static async System.Threading.Tasks.Task RunAsync(
            [ServiceBusTrigger("northeastandcumbria", "northeastandcumbria", AccessRights.Listen, Connection = "ServiceBusConnectionString")]string ServiceBusMessage, 
            TraceWriter log)
        {
            MessagePushService messagePushService = new MessagePushService();
            await messagePushService.PushToTouchpoint(ServiceBusMessage);
        }

    }
}
