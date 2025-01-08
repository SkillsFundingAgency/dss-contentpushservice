using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.ContentPushService.PushService;

namespace NCS.DSS.ContentPushService.Listeners;

public class ListenersHelper : IListenersHelper
{
    private readonly IMessagePushService _messagepushService;
    private readonly ILogger<ListenersHelper> _logger;

    public ListenersHelper(IMessagePushService messagePushService, ILogger<ListenersHelper> logger)
    {
        _messagepushService = messagePushService;
        _logger = logger;
    }

    public async Task SendMessageAsync(ServiceBusReceivedMessage serviceBusMessage, string touchPointId,
        ServiceBusMessageActions messageActions)
    {
        try
        {
            _logger.LogInformation("Attempting to push message to TouchPoint ID: {TouchPointID}", touchPointId);
            await _messagepushService.PushToTouchpoint(
                touchPointId,
                serviceBusMessage,
                messageActions);
            _logger.LogInformation("Finished pushing message to TouchPoint ID: {TouchPointID}", touchPointId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred when attempting to push a service bus message to TouchPoint ID: {TouchPointID}. Exception: {Message}", touchPointId, ex.Message);
        }
    }
}