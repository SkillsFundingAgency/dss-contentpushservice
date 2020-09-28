using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Logging;
using NCS.DSS.ContentPushService.Constants;
using NCS.DSS.ContentPushService.Models;
using System.Threading.Tasks;

namespace NCS.DSS.ContentPushService.Services
{
    public interface IDigitialIdentityService
    {
        Task<DigitalIdentityServiceActions> SendMessage(string topic, Message message, IMessageReceiverService messageReceiver, ILogger logger);
    }
}
