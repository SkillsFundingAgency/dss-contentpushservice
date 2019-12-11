using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using NCS.DSS.ContentPushService.Models;

namespace NCS.DSS.ContentPushService.Listeners
{
    public class CareersHelplineTopicListener
    {
        private const string TopicName = "careershelpline";
        private const string SubscriptionName = "careershelpline";
        private const string AppIdUri = "CareersHelpline.AppIdUri";
        private const string ClientUrl = "CareersHelpline.Url";
        private const string FunctionName = "CareersHelplineTopicListener";
        private const string ServiceBusConnectionString = "ServiceBusConnectionString";

        private readonly IListenersHelper _listenersHelper;

        public CareersHelplineTopicListener(IListenersHelper listenersHelper)
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
