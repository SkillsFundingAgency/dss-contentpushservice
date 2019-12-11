using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using NCS.DSS.ContentPushService.Models;

namespace NCS.DSS.ContentPushService.Listeners
{
    public class EastAndNorthamptonTopicListener
    {
        private const string ServiceBusConnectionString = "ServiceBusConnectionString";
        private const string FunctionName = "EastAndNorthamptonTopicListener";
        private const string TopicName = "eastandnorthampton";
        private const string SubscriptionName = "eastandnorthampton";
        private const string AppIdUri = "EastAndNorthampton.AppIdUri";
        private const string ClientUrl = "EastAndNorthampton.Url";
        private readonly IListenersHelper _listenersHelper;

        public EastAndNorthamptonTopicListener(IListenersHelper listenersHelper)
        {
            _listenersHelper = listenersHelper;
        }

        [FunctionName(FunctionName)]
        public async Task RunAsync([ServiceBusTrigger(TopicName, SubscriptionName, Connection = ServiceBusConnectionString)]Message serviceBusMessage, ILogger log)
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