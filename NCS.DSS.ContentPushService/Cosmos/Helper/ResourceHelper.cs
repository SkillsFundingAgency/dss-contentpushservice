using System;
using NCS.DSS.ContentPushService.Cosmos.Provider;

namespace NCS.DSS.ContentPushService.Cosmos.Helper
{
    public class ResourceHelper : IResourceHelper
    {
        public bool DoesNotificationExist(Guid notificationId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var doesNotificationExist = documentDbProvider.DoesNotificationResourceExist(notificationId);

            return doesNotificationExist;
        }

    }
}
