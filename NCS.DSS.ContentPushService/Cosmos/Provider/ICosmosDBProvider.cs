using Microsoft.Azure.Cosmos;
using NCS.DSS.ContentPushService.Models;

namespace NCS.DSS.ContentPushService.Cosmos.Provider
{
    public interface ICosmosDBProvider
    {
        Task<ItemResponse<DBNotification>> CreateNotificationAsync(DBNotification notification);
    }
}