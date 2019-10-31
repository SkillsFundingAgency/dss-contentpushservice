using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.ServiceBus.Messaging;
using NCS.DSS.ContentPushService.Models;

namespace NCS.DSS.ContentPushService.Listeners
{
    public class SouthEastTopicListener
    {
        private const string TopicName = "southeast";
        private const string SubscriptionName = "southeast";
        private const string AppIdUri = "SouthEast.AppIdUri";
        private const string ClientUrl = "SouthEast.Url";
        private const string FunctionName = "SouthEastTopicListener";
        private const string ServiceBusConnectionString = "ServiceBusConnectionString";
        private readonly IListenersHelper _listenersHelper;

        public SouthEastTopicListener(IListenersHelper listenersHelper)
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
