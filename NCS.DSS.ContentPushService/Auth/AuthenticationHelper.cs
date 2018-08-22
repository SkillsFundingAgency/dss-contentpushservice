using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace NCS.DSS.ContentPushService.Auth
{
    public class AuthenticationHelper
    {
       
        public static async Task<string> GetAccessToken(string clientId, string clientSecret)
        {
            var clientCredential = new ClientCredential(clientId, clientSecret);
           
            var resource = ConfigurationManager.AppSettings["Authentication.ResourceId"];
            var tenant = ConfigurationManager.AppSettings["Authentication.Tenant"];
            var authority = string.Concat(resource, tenant);

            var authContext = new AuthenticationContext(authority);
            
            var authenticationResult = await authContext.AcquireTokenAsync(resource, clientCredential);
            if (authenticationResult != null && !string.IsNullOrWhiteSpace(authenticationResult.AccessToken))
            {
                return authenticationResult.AccessToken;
            }

            return string.Empty;
        }

    }
}
