using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using NCS.DSS.ContentPushService.Models;
using System.Threading.Tasks;

namespace NCS.DSS.ContentPushService.Listeners
{
    public class Touchpoint9000000003TopicListener : TopicListenerBase
    {
        private const string SubscriptionName = "9000000003";
        private const string TopicName = "9000000003";
        private const string AppIdUri = "Touchpoint9000000003.AppIdUri";
        private const string ClientUrl = "Touchpoint9000000003.Url";
        private const string FunctionName = "Touchpoint9000000003";
        private const string ServiceBusConnectionString = "ServiceBusConnectionString";
        private readonly IListenersHelper _listenersHelper;

        public Touchpoint9000000003TopicListener(IListenersHelper listenersHelper)
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
