using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.ServiceBus.Messaging;
using NCS.DSS.ContentPushService.Models;

namespace NCS.DSS.ContentPushService.Listeners
{
    public class LondonTopicListener
    {
        private const string TopicName = "london";
        private const string SubscriptionName = "london";
        private const string AppIdUri = "London.AppIdUri";
        private const string ClientUrl = "London.Url";
        private const string FunctionName = "LondonTopicListener";
        private const string ServiceBusConnectionString = "ServiceBusConnectionString";
        private readonly IListenersHelper _listenersHelper;

        public LondonTopicListener(IListenersHelper listenersHelper)
        {
            _listenersHelper = listenersHelper;
        }

        [FunctionName(FunctionName)]
        public async Task RunAsync([ServiceBusTrigger(TopicName, SubscriptionName, Connection = ServiceBusConnectionString)]BrokeredMessage serviceBusMessage, ILogger log)
        {
            var listinerSettings = new ListenerSettings
            {
                AppIdUri = AppIdUri,
                ClientUrl = ClientUrl,
                SubscriptionName = SubscriptionName,
                TopicName = TopicName
            };

            await _listenersHelper.SendMessageAsync(serviceBusMessage, listinerSettings, log);
        }
    }
}