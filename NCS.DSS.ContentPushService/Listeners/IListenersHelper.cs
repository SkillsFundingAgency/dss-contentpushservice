using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace NCS.DSS.ContentPushService.Listeners;

public interface IListenersHelper
{
    Task SendMessageAsync(ServiceBusReceivedMessage serviceBusMessage, string touchPointId,
        ServiceBusMessageActions messageActions);
}