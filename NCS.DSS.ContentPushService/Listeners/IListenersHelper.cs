using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.ServiceBus.Messaging;
using NCS.DSS.ContentPushService.Models;

namespace NCS.DSS.ContentPushService.Listeners
{
    public interface IListenersHelper
    {
        Task SendMessageAsync(BrokeredMessage serviceBusMessage, ListenerSettings listenerSettings, ILogger log);
    }
}