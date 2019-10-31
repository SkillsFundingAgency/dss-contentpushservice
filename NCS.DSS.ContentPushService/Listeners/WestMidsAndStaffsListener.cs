using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.ServiceBus.Messaging;
using NCS.DSS.ContentPushService.Models;

namespace NCS.DSS.ContentPushService.Listeners
{
    public class WestMidsAndStaffsTopicListener
    {
        private const string TopicName = "westmidsandstaffs";
        private const string SubscriptionName = "westmidsandstaffs";
        private const string AppIdUri = "WestMidsAndStaffs.AppIdUri";
        private const string ClientUrl = "WestMidsAndStaffs.Url";
        private const string FunctionName = "WestMidsAndStaffsTopicListener";
        private const string ServiceBusConnectionString = "ServiceBusConnectionString";
        private readonly IListenersHelper _listenersHelper;

        public WestMidsAndStaffsTopicListener(IListenersHelper listenersHelper)
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
