using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using NCS.DSS.ContentPushService.Models;
using System.Threading.Tasks;

namespace NCS.DSS.ContentPushService.Listeners
{
    public class YorkshireAndHumberTopicListener
    {
        private const string SubscriptionName = "yorkshireandhumber";
        private const string TopicName = "yorkshireandhumber";
        private const string AppIdUri = "YorkshireAndHumber.AppIdUri";
        private const string ClientUrl = "YorkshireAndHumber.Url";
        private const string FunctionName = "YorkshireAndHumberTopicListener";
        private const string ServiceBusConnectionString = "ServiceBusConnectionString";
        private readonly IListenersHelper _listenersHelper;

        public YorkshireAndHumberTopicListener(IListenersHelper listenersHelper)
        {
            _listenersHelper = listenersHelper;
        }

        [FunctionName(FunctionName)]
        public async Task RunAsync([ServiceBusTrigger(TopicName, SubscriptionName, Connection = ServiceBusConnectionString)]Message serviceBusMessage, MessageReceiver messageReceiver, ILogger log)
        {
            var listinerSettings = new ListenerSettings
            {
                AppIdUri = AppIdUri,
                ClientUrl = ClientUrl,
                SubscriptionName = SubscriptionName,
                TopicName = TopicName
            };

            await _listenersHelper.SendMessageAsync(serviceBusMessage, listinerSettings, messageReceiver, log);
        }
    }
}
