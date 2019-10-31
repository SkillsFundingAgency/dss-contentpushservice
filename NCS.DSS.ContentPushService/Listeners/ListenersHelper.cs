using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.ServiceBus.Messaging;
using NCS.DSS.ContentPushService.Models;
using NCS.DSS.ContentPushService.PushService;

namespace NCS.DSS.ContentPushService.Listeners
{
    public class ListenersHelper : IListenersHelper
    {
        public async Task SendMessageAsync(BrokeredMessage serviceBusMessage, ListenerSettings listenerSettings, ILogger log)
        {
            try
            {
                var messagePushService = new MessagePushService();

                await messagePushService.PushToTouchpoint(listenerSettings.AppIdUri, listenerSettings.ClientUrl, serviceBusMessage, listenerSettings.TopicName, log);
                log.LogInformation($"The { listenerSettings.TopicName} topic successfully pushed a notification to { listenerSettings.ClientUrl} at {DateTime.UtcNow}.");
            }
            catch (Exception ex)
            {
                log.LogError($"Unexpected exception in {nameof(SendMessageAsync)}.", ex);
            }
        }
    }
}
