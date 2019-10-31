using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.ServiceBus.Messaging;

namespace NCS.DSS.ContentPushService.PushService
{
    public interface IMessagePushService
    {
        Task PushToTouchpoint(string AppIdUri, string ClientUrl, BrokeredMessage message, string TopicName, ILogger log);
    }
}