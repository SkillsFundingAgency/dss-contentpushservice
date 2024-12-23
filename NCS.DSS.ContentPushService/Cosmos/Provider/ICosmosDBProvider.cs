using Microsoft.Azure.Documents;
using Microsoft.Azure.Cosmos;

namespace NCS.DSS.ContentPushService.Cosmos.Provider
{
    public interface ICosmosDBProvider
    {
        Task<bool> DoesNotificationResourceExist(Guid notificationid);
        Task<ItemResponse<Models.DBNotification>> CreateNotificationAsync(Models.DBNotification notification);
    }
}