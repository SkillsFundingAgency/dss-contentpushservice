using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.ContentPushService.PushService;

namespace NCS.DSS.ContentPushService.Listeners;

public class ListenersHelper : IListenersHelper
{
    private readonly IMessagePushService _messagepushService;

    public ListenersHelper(IMessagePushService messagePushService)
    {
        _messagepushService = messagePushService;
    }

    public async Task SendMessageAsync(ServiceBusReceivedMessage serviceBusMessage, string touchPointId,
        ServiceBusMessageActions messageActions, ILogger log)
    {
        try
        {
            await _messagepushService.PushToTouchpoint(
                touchPointId,
                serviceBusMessage,
                messageActions,
                log);
        }
        catch (Exception ex)
        {
            log.LogError($"Unexpected exception in {nameof(SendMessageAsync)}.", ex);
        }
    }
}