using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Logging;
using NCS.DSS.ContentPushService.Models;

namespace NCS.DSS.ContentPushService.Listeners
{
    public interface IListenersHelper
    {
        Task SendMessageAsync(Message serviceBusMessage, ListenerSettings listenerSettings, MessageReceiver messageReceiver, ILogger log);
    }
}