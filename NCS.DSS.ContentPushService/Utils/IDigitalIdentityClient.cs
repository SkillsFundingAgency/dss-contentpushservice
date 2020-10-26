using System.Threading.Tasks;

namespace NCS.DSS.ContentPushService.Utils
{
    public interface IDigitalIdentityClient
    {
        Task<bool> Post(string body, string endpoint);
        Task<bool> Patch(string body, string endpoint);
        Task<bool> Delete(string body, string endpoint);
        Task<bool> Put(string body, string endpoint);
    }
}
