using System;

namespace NCS.DSS.ContentPushService.Cosmos.Helper
{
    public interface IDocumentDBHelper
    {
        Uri CreateDocumentCollectionUri();
        Uri CreateNotificationsDocumentCollectionUri();
    }
}