using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Logging;
using NCS.DSS.ContentPushService.Models;

namespace NCS.DSS.ContentPushService.Listeners
{
    public interface IListenersHelper
    {
        Task SendMessageAsync(ServiceBusReceivedMessage serviceBusMessage, string touchPointId, MessageReceiver messageReceiver, ILogger log);
    }
}