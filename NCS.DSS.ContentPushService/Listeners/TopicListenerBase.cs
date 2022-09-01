using NCS.DSS.ContentPushService.Models;

namespace NCS.DSS.ContentPushService.Listeners
{
    public class TopicListenerBase
    {
        public static ListenerSettings GetListinerSettings(string AppIdUri, string ClientUrl, string SubscriptionName, string TopicName)
        {
            return new ListenerSettings
            {
                AppIdUri = AppIdUri,
                ClientUrl = ClientUrl,
                SubscriptionName = SubscriptionName,
                TopicName = TopicName
            };
        }
    }
}