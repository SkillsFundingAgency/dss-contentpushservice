using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using NCS.DSS.ContentPushService.Cosmos.Client;
using NCS.DSS.ContentPushService.Cosmos.Helper;

namespace NCS.DSS.ContentPushService.Cosmos.Provider
{
    public class DocumentDBProvider : IDocumentDBProvider
    {
        private readonly DocumentDBHelper _documentDbHelper;
        private readonly DocumentDBClient _databaseClient;

        public DocumentDBProvider()
        {
            _documentDbHelper = new DocumentDBHelper();
            _databaseClient = new DocumentDBClient();
        }

        public bool DoesNotificationResourceExist(Guid notificationId)
        {
            var collectionUri = _documentDbHelper.CreateNotificationsDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return false;

            var qQry = client.CreateDocumentQuery<Document>(collectionUri, new FeedOptions() { MaxItemCount = 1 });
            return qQry.Where(x => x.Id == notificationId.ToString()).Select(x => x.Id).AsEnumerable().Any();
        }


        public async Task<ResourceResponse<Document>> CreateNotificationAsync(Models.DBNotification dbNotification)
        {

            var collectionUri = _documentDbHelper.CreateNotificationsDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return null;

            var response = await client.CreateDocumentAsync(collectionUri, dbNotification);

            return response;

        }

    }
}