using Microsoft.Azure.ServiceBus;
using System.Threading.Tasks;

namespace NCS.DSS.ContentPushService.Services
{
    public interface IRequeueService
    {
        Task<bool> RequeueItem(string topicName, int maxRetryCount, Message message);
    }
}
