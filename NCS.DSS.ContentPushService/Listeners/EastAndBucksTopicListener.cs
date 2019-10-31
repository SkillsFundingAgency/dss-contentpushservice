using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.ServiceBus.Messaging;
using NCS.DSS.ContentPushService.Models;

namespace NCS.DSS.ContentPushService.Listeners
{
    public class EastAndBucksTopicListener
    {
        private const string TopicName = "eastandbucks";
        private const string SubscriptionName = "eastandbucks";
        private const string AppIdUri = "EastAndBucks.AppIdUri";
        private const string ClientUrl = "EastAndBucks.Url";
        private const string FunctionName = "EastAndBucksTopicListener";
        private const string ServiceBusConnectionString = "ServiceBusConnectionString";
        private readonly IListenersHelper _listenersHelper;

        public EastAndBucksTopicListener(IListenersHelper listenersHelper)
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

            await _listenersHelper.SendMessageAsync(serviceBusMessage, listinerSettings , log);
        }
    }
}