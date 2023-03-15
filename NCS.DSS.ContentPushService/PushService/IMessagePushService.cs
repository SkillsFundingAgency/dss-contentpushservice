using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Extensions.Logging;

namespace NCS.DSS.ContentPushService.PushService;

public interface IMessagePushService
{
    Task PushToTouchpoint(string touchpoint, ServiceBusReceivedMessage message, ServiceBusMessageActions messageActions,
        ILogger log);
}