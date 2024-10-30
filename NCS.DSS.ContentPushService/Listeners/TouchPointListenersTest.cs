using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace NCS.DSS.ContentPushService.Listeners;

public class TouchPointListenersTest
{
    private const string ServiceBusConnectionString = "ServiceBusConnectionString";
    public const string TP_9000000001 = "9000000001";
    public const string TP_9000000002 = "9000000002";
    public const string TP_9000000003 = "9000000003";
    private readonly IListenersHelper _listenersHelper;
    private readonly ILogger _logger;

    public TouchPointListenersTest(IListenersHelper listenersHelper, ILogger<TouchPointListenersTest> logger)
    {
        _listenersHelper = listenersHelper;
        _logger = logger;
    }

    [Function("TOUCHPOINT_" + TP_9000000001)]
    public async Task TouchPoint_9000000001(
        [ServiceBusTrigger(TP_9000000001, TP_9000000001, Connection = ServiceBusConnectionString)]
        ServiceBusReceivedMessage serviceBusMessage, ServiceBusMessageActions messageActions)
    {
        await _listenersHelper.SendMessageAsync(serviceBusMessage, TP_9000000001, messageActions, _logger);
    }

    [Function("TOUCHPOINT_" + TP_9000000002)]
    public async Task TouchPoint_9000000002(
        [ServiceBusTrigger(TP_9000000002, TP_9000000002, Connection = ServiceBusConnectionString)]
        ServiceBusReceivedMessage serviceBusMessage, ServiceBusMessageActions messageActions)
    {
        await _listenersHelper.SendMessageAsync(serviceBusMessage, TP_9000000002, messageActions, _logger);
    }

    [Function("TOUCHPOINT_" + TP_9000000003)]
    public async Task TouchPoint_9000000003(
        [ServiceBusTrigger(TP_9000000003, TP_9000000003, Connection = ServiceBusConnectionString)]
        ServiceBusReceivedMessage serviceBusMessage, ServiceBusMessageActions messageActions)
    {
        await _listenersHelper.SendMessageAsync(serviceBusMessage, TP_9000000003, messageActions, _logger);
    }
}