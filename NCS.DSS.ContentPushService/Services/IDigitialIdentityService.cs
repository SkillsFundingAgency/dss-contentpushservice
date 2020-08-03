using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Logging;
using NCS.DSS.ContentPushService.Models;
using System.Threading.Tasks;

namespace NCS.DSS.ContentPushService.Services
{
    public interface IDigitialIdentityService
    {
        Task SendMessage(string topic, string connectionString, Message message, ListenerSettings listenerSettings, IMessageReceiver messageReceiver, ILogger logger);
    }
}
