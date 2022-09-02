using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace NCS.DSS.ContentPushService.Listeners
{
    public class Touchpoint0000000107TopicListener : TopicListenerBase
    {
        private const string SubscriptionName = "0000000107";
        private const string TopicName = "0000000107";
        private const string AppIdUri = "Touchpoint0000000107.AppIdUri";
        private const string ClientUrl = "Touchpoint0000000107.Url";
        private const string FunctionName = "Touchpoint0000000107";
        private const string ServiceBusConnectionString = "ServiceBusConnectionString";
        private readonly IListenersHelper _listenersHelper;

        public Touchpoint0000000107TopicListener(IListenersHelper listenersHelper)
        {
            _listenersHelper = listenersHelper;
        }

        [FunctionName(FunctionName)]
        public async Task RunAsync([ServiceBusTrigger(TopicName, SubscriptionName, Connection = ServiceBusConnectionString)]Message serviceBusMessage, MessageReceiver messageReceiver, ILogger log)
        {
            var listinerSettings = GetListinerSettings(AppIdUri, ClientUrl, SubscriptionName, TopicName);
            await _listenersHelper.SendMessageAsync(serviceBusMessage, listinerSettings, messageReceiver, log);
        }
    }
}