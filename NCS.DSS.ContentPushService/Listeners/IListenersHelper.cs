using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using NCS.DSS.ContentPushService.Models;

namespace NCS.DSS.ContentPushService.Listeners
{
    public interface IListenersHelper
    {
        Task SendMessageAsync(Message serviceBusMessage, ListenerSettings listenerSettings, ILogger log);
    }
}