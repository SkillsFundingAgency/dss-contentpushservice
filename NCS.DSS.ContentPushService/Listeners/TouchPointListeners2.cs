using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace NCS.DSS.ContentPushService.Listeners;

public class TouchPointListeners2
{
    private const string ServiceBusConnectionString = "ServiceBusConnectionString";

    public const string TP_0000000201 = "0000000201";
    public const string TP_0000000202 = "0000000202";
    public const string TP_0000000203 = "0000000203";
    public const string TP_0000000204 = "0000000204";
    public const string TP_0000000205 = "0000000205";
    public const string TP_0000000206 = "0000000206";
    public const string TP_0000000207 = "0000000207";
    public const string TP_0000000208 = "0000000208";
    public const string TP_0000000209 = "0000000209";
    private readonly IListenersHelper _listenersHelper;
    private readonly ILogger _logger;

    public TouchPointListeners2(IListenersHelper listenersHelper, ILogger<TouchPointListeners2> logger)
    {
        _listenersHelper = listenersHelper;
        _logger = logger;
    }

    [Function("TOUCHPOINT_" + TP_0000000201)]
    public async Task TouchPoint_0000000201(
        [ServiceBusTrigger(TP_0000000201, TP_0000000201, Connection = ServiceBusConnectionString)]
        ServiceBusReceivedMessage serviceBusMessage, ServiceBusMessageActions messageActions)
    {
        await _listenersHelper.SendMessageAsync(serviceBusMessage, TP_0000000201, messageActions, _logger);
    }

    [Function("TOUCHPOINT_" + TP_0000000202)]
    public async Task TouchPoint_0000000202(
        [ServiceBusTrigger(TP_0000000202, TP_0000000202, Connection = ServiceBusConnectionString)]
        ServiceBusReceivedMessage serviceBusMessage, ServiceBusMessageActions messageActions)
    {
        await _listenersHelper.SendMessageAsync(serviceBusMessage, TP_0000000202, messageActions, _logger);
    }

    [Function("TOUCHPOINT_" + TP_0000000203)]
    public async Task TouchPoint_0000000203(
        [ServiceBusTrigger(TP_0000000203, TP_0000000203, Connection = ServiceBusConnectionString)]
        ServiceBusReceivedMessage serviceBusMessage, ServiceBusMessageActions messageActions)
    {
        await _listenersHelper.SendMessageAsync(serviceBusMessage, TP_0000000203, messageActions, _logger);
    }

    [Function("TOUCHPOINT_" + TP_0000000204)]
    public async Task TouchPoint_0000000204(
        [ServiceBusTrigger(TP_0000000204, TP_0000000204, Connection = ServiceBusConnectionString)]
        ServiceBusReceivedMessage serviceBusMessage, ServiceBusMessageActions messageActions)
    {
        await _listenersHelper.SendMessageAsync(serviceBusMessage, TP_0000000204, messageActions, _logger);
    }

    [Function("TOUCHPOINT_" + TP_0000000205)]
    public async Task TouchPoint_0000000205(
        [ServiceBusTrigger(TP_0000000205, TP_0000000205, Connection = ServiceBusConnectionString)]
        ServiceBusReceivedMessage serviceBusMessage, ServiceBusMessageActions messageActions)
    {
        await _listenersHelper.SendMessageAsync(serviceBusMessage, TP_0000000205, messageActions, _logger);
    }

    [Function("TOUCHPOINT_" + TP_0000000206)]
    public async Task TouchPoint_0000000206(
        [ServiceBusTrigger(TP_0000000206, TP_0000000206, Connection = ServiceBusConnectionString)]
        ServiceBusReceivedMessage serviceBusMessage, ServiceBusMessageActions messageActions)
    {
        await _listenersHelper.SendMessageAsync(serviceBusMessage, TP_0000000206, messageActions, _logger);
    }

    [Function("TOUCHPOINT_" + TP_0000000207)]
    public async Task TouchPoint_0000000207(
        [ServiceBusTrigger(TP_0000000207, TP_0000000207, Connection = ServiceBusConnectionString)]
        ServiceBusReceivedMessage serviceBusMessage, ServiceBusMessageActions messageActions)
    {
        await _listenersHelper.SendMessageAsync(serviceBusMessage, TP_0000000207, messageActions, _logger);
    }

    [Function("TOUCHPOINT_" + TP_0000000208)]
    public async Task TouchPoint_0000000208(
        [ServiceBusTrigger(TP_0000000208, TP_0000000208, Connection = ServiceBusConnectionString)]
        ServiceBusReceivedMessage serviceBusMessage, ServiceBusMessageActions messageActions)
    {
        await _listenersHelper.SendMessageAsync(serviceBusMessage, TP_0000000208, messageActions, _logger);
    }

    [Function("TOUCHPOINT_" + TP_0000000209)]
    public async Task TouchPoint_0000000209(
        [ServiceBusTrigger(TP_0000000209, TP_0000000209, Connection = ServiceBusConnectionString)]
        ServiceBusReceivedMessage serviceBusMessage, ServiceBusMessageActions messageActions)
    {
        await _listenersHelper.SendMessageAsync(serviceBusMessage, TP_0000000209, messageActions, _logger);
    }
}