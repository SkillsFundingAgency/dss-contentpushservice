using Microsoft.Azure.ServiceBus;
using NCS.DSS.ContentPushService.Constants;
using System.Threading.Tasks;

namespace NCS.DSS.ContentPushService.Services
{
    public interface IDigitialIdentityService
    {
        Task<DigitalIdentityServiceActions> SendMessage(string topic, Message message, IMessageReceiverService messageReceiver);
    }
}
