using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using NCS.DSS.ContentPushService.Cosmos.Provider;

namespace NCS.DSS.ContentPushService.Cosmos.Helper
{
    public class ResourceHelper : IResourceHelper
    {
        private readonly ICosmosDBProvider _cosmosDbProvider;
        
        public ResourceHelper(ICosmosDBProvider cosmosDBProvider)
        {
            _cosmosDbProvider = cosmosDBProvider;
        }
        
        public Task<bool> DoesNotificationExist(Guid notificationId, CosmosClient cosmosClient, ILogger<CosmosDBProvider> log)
        {
            //Logging done within DoesNotificationResourceExist method
            var doesNotificationExist = _cosmosDbProvider.DoesNotificationResourceExist(notificationId);

            return doesNotificationExist;
        }

    }
}
