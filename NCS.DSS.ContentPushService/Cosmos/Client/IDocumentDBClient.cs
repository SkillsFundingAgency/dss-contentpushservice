using Microsoft.Azure.Documents.Client;

namespace NCS.DSS.ContentPushService.Cosmos.Client
{
    public interface IDocumentDBClient
    {
        DocumentClient CreateDocumentClient();
    }
}