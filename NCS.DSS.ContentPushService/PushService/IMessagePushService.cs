using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;

namespace NCS.DSS.ContentPushService.PushService;

public interface IMessagePushService
{
    Task PushToTouchpoint(string touchpoint, ServiceBusReceivedMessage message, ServiceBusMessageActions messageActions);
}