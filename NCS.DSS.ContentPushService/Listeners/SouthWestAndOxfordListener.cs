using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using NCS.DSS.ContentPushService.Models;

namespace NCS.DSS.ContentPushService.Listeners
{
    public class SouthWestAndOxfordTopicListener
    {
        private const string TopicName = "southwestandoxford";
        private const string SubscriptionName = "southwestandoxford";
        private const string AppIdUri = "SouthWestAndOxford.AppIdUri";
        private const string ClientUrl = "SouthWestAndOxford.Url";
        private const string FunctionName = "SouthWestAndOxfordTopicListener";
        private const string ServiceBusConnectionString = "ServiceBusConnectionString";
        private readonly IListenersHelper _listenersHelper;

        public SouthWestAndOxfordTopicListener(IListenersHelper listenersHelper)
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
