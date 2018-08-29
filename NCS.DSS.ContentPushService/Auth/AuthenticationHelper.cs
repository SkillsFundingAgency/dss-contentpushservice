using System.Configuration;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace NCS.DSS.ContentPushService.Auth
{
    public class AuthenticationHelper
    {
       public static async Task<string> GetAccessToken(string clientId, string clientSecret)
        {
            var clientCredential = new ClientCredential(clientId, clientSecret);
           
            var authorityUri = ConfigurationManager.AppSettings["Authentication.AuthorityUri"];
            var tenant = ConfigurationManager.AppSettings["Authentication.Tenant"];

            var authority = string.Concat(authorityUri, tenant);

            var authContext = new AuthenticationContext(authority);

            var appIdUri = ConfigurationManager.AppSettings["Authentication.AppIdUri"];
            
            var authenticationResult = await authContext.AcquireTokenAsync(appIdUri, clientCredential);
            if (authenticationResult != null && !string.IsNullOrWhiteSpace(authenticationResult.AccessToken))
            {
                return authenticationResult.AccessToken;
            }

            return string.Empty;
        }
    }
}
