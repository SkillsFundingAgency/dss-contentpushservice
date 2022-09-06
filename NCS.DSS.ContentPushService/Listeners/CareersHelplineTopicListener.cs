using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using NCS.DSS.ContentPushService.Models;

namespace NCS.DSS.ContentPushService.Listeners
{
    public class CareersHelplineTopicListener
    {
        private readonly IListenersHelper _listenersHelper;
        private const string ServiceBusConnectionString = "ServiceBusConnectionString";
        public const string TP_0000000999 = "0000000999";

        public CareersHelplineTopicListener(IListenersHelper listenersHelper)
        {
            _listenersHelper = listenersHelper;
        }

        [FunctionName("TOUCHPOINT_" + TP_0000000999)]
        public async Task TouchPoint_0000000999(
            [ServiceBusTrigger(TP_0000000999, TP_0000000999, Connection = ServiceBusConnectionString)]
            Message serviceBusMessage, MessageReceiver messageReceiver, ILogger log)
        {
            await _listenersHelper.SendMessageAsync(serviceBusMessage, TP_0000000999, messageReceiver, log);
        }
    }
}
