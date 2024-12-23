using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using NCS.DSS.ContentPushService.Cosmos.Provider;

namespace NCS.DSS.ContentPushService.Cosmos.Helper
{
    public interface IResourceHelper
    {
        Task<bool> DoesNotificationExist(Guid notificationId, CosmosClient cosmosClient, ILogger<CosmosDBProvider> log);
    }
}