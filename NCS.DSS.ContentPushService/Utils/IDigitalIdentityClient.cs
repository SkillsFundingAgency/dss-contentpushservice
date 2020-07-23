using System.Threading.Tasks;

namespace NCS.DSS.ContentPushService.Utils
{
    public interface IDigitalIdentityClient
    {
        Task<bool> Post<T>(T t, string endpoint);
        Task<bool> Patch<T>(T t, string endpoint);
    }
}
