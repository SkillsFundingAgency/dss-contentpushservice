namespace NCS.DSS.ContentPushService.Utils
{
    public interface IDigitalIdentityClient
    {
        Task<bool> Post(string customerId, string objectId, string body, string endpoint);
        Task<bool> Patch(string customerId, string objectId, string body, string endpoint);
        Task<bool> Delete(string customerId, string objectId, string body, string endpoint);
        Task<bool> Put(string customerId, string objectId, string body, string endpoint);
    }
}
