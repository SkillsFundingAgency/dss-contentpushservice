namespace NCS.DSS.ContentPushService.Models
{
    public class ContentPushServiceConfigurationSettings
    {
        public required string AuthenticationAuthorityUri { get; set; }
        public required string AuthenticationPushServiceClientId { get; set; }
        public required string AuthenticationPushServiceClientSecret { get; set; }
        public required string AuthenticationTenant { get; set; }

        // - AZURE B2C -

        public required string AzureB2CApiUrl { get; set; }
        public required string AzureB2CApiKey { get; set; }
        public required string AzureB2CTenant { get; set; }

        // - AZURE COSMOS DB -

        public required string CollectionId { get; set; }
        public required string DatabaseId { get; set; }
        public required string Endpoint { get; set; }
        public required string EnvironmentName { get; set; }
        public required string Key { get; set; }
        public required string NotificationCollectionId { get; set; }
        public required string NotificationDatabaseId { get; set; }
        public required string ServiceBusConnectionString { get; set; }

        // - '100' SERIES OF TOUCHPOINT LISTENER -

        public required string TouchPoint0000000101AppIdUri { get; set; }
        public required string TouchPoint0000000101Url { get; set; }
        public required string TouchPoint0000000102AppIdUri { get; set; }
        public required string TouchPoint0000000102Url { get; set; }
        public required string TouchPoint0000000103AppIdUri { get; set; }
        public required string TouchPoint0000000103Url { get; set; }
        public required string TouchPoint0000000104AppIdUri { get; set; }
        public required string TouchPoint0000000104Url { get; set; }
        public required string TouchPoint0000000105AppIdUri { get; set; }
        public required string TouchPoint0000000105Url { get; set; }
        public required string TouchPoint0000000106AppIdUri { get; set; }
        public required string TouchPoint0000000106Url { get; set; }
        public required string TouchPoint0000000107AppIdUri { get; set; }
        public required string TouchPoint0000000107Url { get; set; }
        public required string TouchPoint0000000108AppIdUri { get; set; }
        public required string TouchPoint0000000108Url { get; set; }
        public required string TouchPoint0000000109AppIdUri { get; set; }
        public required string TouchPoint0000000109Url { get; set; }

        // - '200' SERIES OF TOUCHPOINT LISTENER -

        public required string TouchPoint0000000201AppIdUri { get; set; }
        public required string TouchPoint0000000201Url { get; set; }
        public required string TouchPoint0000000202AppIdUri { get; set; }
        public required string TouchPoint0000000202Url { get; set; }
        public required string TouchPoint0000000203AppIdUri { get; set; }
        public required string TouchPoint0000000203Url { get; set; }
        public required string TouchPoint0000000204AppIdUri { get; set; }
        public required string TouchPoint0000000204Url { get; set; }
        public required string TouchPoint0000000205AppIdUri { get; set; }
        public required string TouchPoint0000000205Url { get; set; }
        public required string TouchPoint0000000206AppIdUri { get; set; }
        public required string TouchPoint0000000206Url { get; set; }
        public required string TouchPoint0000000207AppIdUri { get; set; }
        public required string TouchPoint0000000207Url { get; set; }
        public required string TouchPoint0000000208AppIdUri { get; set; }
        public required string TouchPoint0000000208Url { get; set; }
        public required string TouchPoint0000000209AppIdUri { get; set; }
        public required string TouchPoint0000000209Url { get; set; }

        // - '999' TOUCHPOINT LISTENER -

        public required string TouchPoint0000000999AppIdUri { get; set; }
        public required string TouchPoint0000000999Url { get; set; }

        // - '900' SERIES OF TOUCHPOINT LISTENER -

        public required string TouchPoint9000000001AppIdUri { get; set; }
        public required string TouchPoint9000000001Url { get; set; }
        public required string TouchPoint9000000002AppIdUri { get; set; }
        public required string TouchPoint9000000002Url { get; set; }
        public required string TouchPoint9000000003AppIdUri { get; set; }
        public required string TouchPoint9000000003Url { get; set; }
    }
}