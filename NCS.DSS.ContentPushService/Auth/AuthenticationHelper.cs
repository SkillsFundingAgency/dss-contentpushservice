using System;
using System.Configuration;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace NCS.DSS.ContentPushService.Auth
{
    public class AuthenticationHelper
    {
       
        public static async Task<AuthenticationResult> GetAccessToken(string clientId, string clientSecret)
        {

            var clientCredential = new ClientCredential(clientId, clientSecret);
           
            var resource = ConfigurationManager.AppSettings["ResourceId"];
            var tenant = ConfigurationManager.AppSettings["Tenant"];
            var authority = string.Concat(resource, tenant);

            var authContext = new AuthenticationContext(authority);

            return await authContext.AcquireTokenAsync(resource, clientCredential);
        }

    }
}
