using System;

namespace NCS.DSS.ContentPushService.Cosmos.Helper
{
    public interface IResourceHelper
    {
        bool DoesNotificationExist(Guid notificationId);
    }
}