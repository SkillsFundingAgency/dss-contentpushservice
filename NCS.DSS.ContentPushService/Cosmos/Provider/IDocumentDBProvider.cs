using System;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace NCS.DSS.ContentPushService.Cosmos.Provider
{
    public interface IDocumentDBProvider
    {
        bool DoesNotificationResourceExist(Guid notificationid);
        Task<ResourceResponse<Document>> CreateNotificationAsync(Models.DBNotification dbNotification);
    }
}