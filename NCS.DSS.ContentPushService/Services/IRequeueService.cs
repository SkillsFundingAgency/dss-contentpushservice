using Azure.Messaging.ServiceBus;

namespace NCS.DSS.ContentPushService.Services;

public interface IRequeueService
{
    Task<bool> RequeueItem(string topicName, int maxRetryCount, ServiceBusReceivedMessage serviceBusReceivedMessage);
}