using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace NCS.DSS.ContentPushService.Listeners;

public class CareersHelplineListener
{
    private const string ServiceBusConnectionString = "ServiceBusConnectionString";
    public const string TP_0000000999 = "0000000999";
    private readonly IListenersHelper _listenersHelper;
    private readonly ILogger<CareersHelplineListener> _logger;

    public CareersHelplineListener(IListenersHelper listenersHelper, ILogger<CareersHelplineListener> logger)
    {
        _listenersHelper = listenersHelper;
        _logger = logger;
    }

    [Function("TOUCHPOINT_" + TP_0000000999)]
    public async Task TouchPoint_0000000999(
        [ServiceBusTrigger(TP_0000000999, TP_0000000999, Connection = ServiceBusConnectionString)]
        ServiceBusReceivedMessage serviceBusMessage, ServiceBusMessageActions messageActions)
    {
        _logger.LogInformation("Sending message to Service Bus");
        await _listenersHelper.SendMessageAsync(serviceBusMessage, TP_0000000999, messageActions);
    }
}