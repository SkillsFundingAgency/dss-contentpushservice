using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Extensions.Logging;

namespace NCS.DSS.ContentPushService.Listeners;

public interface IListenersHelper
{
    Task SendMessageAsync(ServiceBusReceivedMessage serviceBusMessage, string touchPointId,
        ServiceBusMessageActions messageActions, ILogger log);
}