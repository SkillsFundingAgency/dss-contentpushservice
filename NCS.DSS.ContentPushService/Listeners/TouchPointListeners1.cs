using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace NCS.DSS.ContentPushService.Listeners;

public class TouchPointListeners1
{
    private const string ServiceBusConnectionString = "ServiceBusConnectionString";


    public const string TP_0000000101 = "0000000101";
    public const string TP_0000000102 = "0000000102";
    public const string TP_0000000103 = "0000000103";
    public const string TP_0000000104 = "0000000104";
    public const string TP_0000000105 = "0000000105";
    public const string TP_0000000106 = "0000000106";
    public const string TP_0000000107 = "0000000107";
    public const string TP_0000000108 = "0000000108";
    public const string TP_0000000109 = "0000000109";
    private readonly IListenersHelper _listenersHelper;
    private readonly ILogger _logger;


    public TouchPointListeners1(IListenersHelper listenersHelper, ILogger<TouchPointListeners1> logger)
    {
        _listenersHelper = listenersHelper;
        _logger = logger;
    }

    [Function("TOUCHPOINT_" + TP_0000000101)]
    public async Task TouchPoint_0000000101(
        [ServiceBusTrigger(TP_0000000101, TP_0000000101, Connection = ServiceBusConnectionString)]
        ServiceBusReceivedMessage serviceBusMessage, ServiceBusMessageActions messageActions)
    {
        await _listenersHelper.SendMessageAsync(serviceBusMessage, TP_0000000101, messageActions, _logger);
    }

    [Function("TOUCHPOINT_" + TP_0000000102)]
    public async Task TouchPoint_0000000102(
        [ServiceBusTrigger(TP_0000000102, TP_0000000102, Connection = ServiceBusConnectionString)]
        ServiceBusReceivedMessage serviceBusMessage, ServiceBusMessageActions messageActions)
    {
        await _listenersHelper.SendMessageAsync(serviceBusMessage, TP_0000000102, messageActions, _logger);
    }

    [Function("TOUCHPOINT_" + TP_0000000103)]
    public async Task TouchPoint_0000000103(
        [ServiceBusTrigger(TP_0000000103, TP_0000000103, Connection = ServiceBusConnectionString)]
        ServiceBusReceivedMessage serviceBusMessage, ServiceBusMessageActions messageActions)
    {
        await _listenersHelper.SendMessageAsync(serviceBusMessage, TP_0000000103, messageActions, _logger);
    }

    [Function("TOUCHPOINT_" + TP_0000000104)]
    public async Task TouchPoint_0000000104(
        [ServiceBusTrigger(TP_0000000104, TP_0000000104, Connection = ServiceBusConnectionString)]
        ServiceBusReceivedMessage serviceBusMessage, ServiceBusMessageActions messageActions)
    {
        await _listenersHelper.SendMessageAsync(serviceBusMessage, TP_0000000104, messageActions, _logger);
    }

    [Function("TOUCHPOINT_" + TP_0000000105)]
    public async Task TouchPoint_0000000105(
        [ServiceBusTrigger(TP_0000000105, TP_0000000105, Connection = ServiceBusConnectionString)]
        ServiceBusReceivedMessage serviceBusMessage, ServiceBusMessageActions messageActions)
    {
        await _listenersHelper.SendMessageAsync(serviceBusMessage, TP_0000000105, messageActions, _logger);
    }

    [Function("TOUCHPOINT_" + TP_0000000106)]
    public async Task TouchPoint_0000000106(
        [ServiceBusTrigger(TP_0000000106, TP_0000000106, Connection = ServiceBusConnectionString)]
        ServiceBusReceivedMessage serviceBusMessage, ServiceBusMessageActions messageActions)
    {
        await _listenersHelper.SendMessageAsync(serviceBusMessage, TP_0000000106, messageActions, _logger);
    }

    // messageReceiver 

    [Function("TOUCHPOINT_" + TP_0000000107)]
    public async Task TouchPoint_0000000107(
        [ServiceBusTrigger(TP_0000000107, TP_0000000107, Connection = ServiceBusConnectionString)]
        ServiceBusReceivedMessage serviceBusMessage, ServiceBusMessageActions messageActions)
    {
        await _listenersHelper.SendMessageAsync(serviceBusMessage, TP_0000000107, messageActions, _logger);
    }

    [Function("TOUCHPOINT_" + TP_0000000108)]
    public async Task TouchPoint_0000000108(
        [ServiceBusTrigger(TP_0000000108, TP_0000000108, Connection = ServiceBusConnectionString)]
        ServiceBusReceivedMessage serviceBusMessage, ServiceBusMessageActions messageActions)
    {
        await _listenersHelper.SendMessageAsync(serviceBusMessage, TP_0000000108, messageActions, _logger);
    }

    [Function("TOUCHPOINT_" + TP_0000000109)]
    public async Task TouchPoint_0000000109(
        [ServiceBusTrigger(TP_0000000109, TP_0000000109, Connection = ServiceBusConnectionString)]
        ServiceBusReceivedMessage serviceBusMessage, ServiceBusMessageActions messageActions)
    {
        await _listenersHelper.SendMessageAsync(serviceBusMessage, TP_0000000109, messageActions, _logger);
    }
}