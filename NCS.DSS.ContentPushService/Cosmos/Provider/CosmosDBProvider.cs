using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace NCS.DSS.ContentPushService.Cosmos.Provider
{
    public class CosmosDBProvider : ICosmosDBProvider
    {
        private readonly Container _notificationsContainer;
        private readonly string _databaseId = Environment.GetEnvironmentVariable("DatabaseId");
        private readonly string _collectionId = Environment.GetEnvironmentVariable("CollectionId");
        private readonly PartitionKey _partitionKey = PartitionKey.None;
        private readonly ILogger<CosmosDBProvider> _logger;

        public CosmosDBProvider(
            CosmosClient cosmosClient,
            ILogger<CosmosDBProvider> logger)
        {
            _notificationsContainer = GetContainer(cosmosClient, _databaseId, _collectionId);
            _logger = logger;
        }

        private static Container GetContainer(CosmosClient cosmosClient, string databaseId, string collectionId)
            => cosmosClient.GetContainer(databaseId, collectionId);

        public async Task<bool> DoesNotificationResourceExist(Guid notificationId)
        {
            try
            {
                _logger.LogInformation("Checking whether the notification resource {NotificationId} exists.", notificationId);
                var response = await _notificationsContainer.ReadItemAsync<Models.DBNotification>(
                    notificationId.ToString(),
                    _partitionKey);

                _logger.LogInformation("Check complete with response code: {0}.", (int)response?.StatusCode);
                return response.Resource != null;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Notification resource {NotificationId} not found.", notificationId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking notification resource existence. Notification ID: {NotificationId}", notificationId);
                throw;
            }
        }

        public async Task<ItemResponse<Models.DBNotification>> CreateNotificationAsync(Models.DBNotification notification)
        {
            try
            {
                _logger.LogInformation("Creating Notification. Notification ID: {NotificationId}", notification.MessageId);

                var response = await _notificationsContainer.CreateItemAsync(
                    notification,
                    _partitionKey);

                _logger.LogInformation("Finished creating Notification. Notification ID: {NotificationId}", notification.MessageId);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when creating notification");
                throw;
            }
        }

    }
}