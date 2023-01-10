using System;
using System.Net.Http;
using System.Resources;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Logging;
using NCS.DSS.ContentPushService.Models;
using NCS.DSS.ContentPushService.PushService;

namespace NCS.DSS.ContentPushService.Listeners
{
    public class ListenersHelper : IListenersHelper
    {
        private readonly IMessagePushService _messagepushService;

        public ListenersHelper(IMessagePushService messagePushService)
        {
            _messagepushService = messagePushService;
        }

        public async Task SendMessageAsync(ServiceBusReceivedMessage serviceBusMessage, string touchPointId, MessageReceiver messageReceiver, ILogger log)
        {
            try
            {
                await _messagepushService.PushToTouchpoint(
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
