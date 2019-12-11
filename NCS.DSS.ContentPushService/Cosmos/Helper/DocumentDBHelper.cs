
using System;
using Microsoft.Azure.Documents.Client;

namespace NCS.DSS.ContentPushService.Cosmos.Helper
{
    public class DocumentDBHelper : IDocumentDBHelper
    {
        private Uri _documentCollectionUri;
        private Uri _documentUri;
        private readonly string _databaseId = Environment.GetEnvironmentVariable("DatabaseId");
        private readonly string _collectionId = Environment.GetEnvironmentVariable("CollectionId");

        private Uri _notificationsDocumentCollectionUri;
        private readonly string _notificationsDatabaseId = Environment.GetEnvironmentVariable("NotificationDatabaseId");
        private readonly string _notificationsCollectionId = Environment.GetEnvironmentVariable("NotificationCollectionId");


        public Uri CreateDocumentCollectionUri()
        {
            if (_documentCollectionUri != null)
                return _documentCollectionUri;

            _documentCollectionUri = UriFactory.CreateDocumentCollectionUri(
                _databaseId,
                _collectionId);

            return _documentCollectionUri;
        }
        
        public Uri CreateDocumentUri(Guid actionPlanId)
        {
            if (_documentUri != null)
                return _documentUri;

            _documentUri = UriFactory.CreateDocumentUri(_databaseId, _collectionId, actionPlanId.ToString());

            return _documentUri;

        }

        #region NotificationsDB

        public Uri CreateNotificationsDocumentCollectionUri()
        {
            if (_notificationsDocumentCollectionUri != null)
                return _notificationsDocumentCollectionUri;

            _notificationsDocumentCollectionUri = UriFactory.CreateDocumentCollectionUri(
                _notificationsDatabaseId, _notificationsCollectionId);

            return _notificationsDocumentCollectionUri;
        }

        #endregion


    }
}
