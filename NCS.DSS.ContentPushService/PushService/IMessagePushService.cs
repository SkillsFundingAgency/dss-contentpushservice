using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace NCS.DSS.ContentPushService.PushService;

public interface IMessagePushService
{
    Task PushToTouchpoint(string touchpoint, ServiceBusReceivedMessage message, ServiceBusMessageActions messageActions,
        ILogger log);
}