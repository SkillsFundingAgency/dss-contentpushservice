using System;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Logging;
using NCS.DSS.ContentPushService.Models;
using NCS.DSS.ContentPushService.PushService;

namespace NCS.DSS.ContentPushService.Listeners
{
    public class ListenersHelper : IListenersHelper
    {
        public async Task SendMessageAsync(Message serviceBusMessage, string touchPointId, MessageReceiver messageReceiver, ILogger log)
        {
            try
            {
                var messagePushService = new MessagePushService();

                await messagePushService.PushToTouchpoint(
                    touchPointId,
                    serviceBusMessage,
                    messageReceiver,
                    log);
            }
            catch (Exception ex)
            {
                log.LogError($"Unexpected exception in {nameof(SendMessageAsync)}.", ex);
            }
        }
    }
}
