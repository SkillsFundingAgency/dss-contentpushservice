using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCS.DSS.ContentPushService.Models;

namespace NCS.DSS.ContentPushService.Cosmos.Provider
{
    public class CosmosDBProvider : ICosmosDBProvider
    {
        private readonly Container _notificationsContainer;
        private readonly ILogger<CosmosDBProvider> _logger;
        private readonly PartitionKey _partitionKey = PartitionKey.None;

        public CosmosDBProvider(
            CosmosClient cosmosClient,
            IOptions<ContentPushServiceConfigurationSettings> configuration,
            ILogger<CosmosDBProvider> logger)
        {
            var config = configuration.Value;

            _logger = logger;
            _notificationsContainer = GetContainer(cosmosClient, config.NotificationDatabaseId, config.NotificationCollectionId);
        }

        private static Container GetContainer(CosmosClient cosmosClient, string databaseId, string collectionId)
            => cosmosClient.GetContainer(databaseId, collectionId);

        public async Task<ItemResponse<DBNotification>> CreateNotificationAsync(DBNotification notification)
        {
            try
            {
                _logger.LogInformation("Creating Notification. Notification ID: {NotificationId}", notification.MessageId);

                var response = await _notificationsContainer.CreateItemAsync(
                    notification,
                    _partitionKey);

                _logger.LogInformation("Finished creating Notification in Cosmos DB. Response code: {response} Notification ID: {NotificationId}", response.StatusCode, notification.MessageId);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when creating notification.");
                throw;
            }
        }

    }
}