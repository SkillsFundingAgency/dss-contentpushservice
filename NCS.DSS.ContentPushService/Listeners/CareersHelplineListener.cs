using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Extensions.Logging;

namespace NCS.DSS.ContentPushService.Listeners;

public class CareersHelplineListener
{
    private const string ServiceBusConnectionString = "ServiceBusConnectionString";
    public const string TP_0000000999 = "0000000999";
    private readonly IListenersHelper _listenersHelper;

    public CareersHelplineListener(IListenersHelper listenersHelper)
    {
        _listenersHelper = listenersHelper;
    }

    [FunctionName("TOUCHPOINT_" + TP_0000000999)]
    public async Task TouchPoint_0000000999(
        [ServiceBusTrigger(TP_0000000999, TP_0000000999, Connection = ServiceBusConnectionString)]
        ServiceBusReceivedMessage serviceBusMessage, ServiceBusMessageActions messageActions, ILogger log)
    {
        await _listenersHelper.SendMessageAsync(serviceBusMessage, TP_0000000999, messageActions, log);
    }
}