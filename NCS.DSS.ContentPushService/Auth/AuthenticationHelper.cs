using System.Configuration;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace NCS.DSS.ContentPushService.Auth
{
    public static class AuthenticationHelper
    {
        public static async Task<string> GetAccessToken(string appIdUri)
        {
            var clientId = ConfigurationManager.AppSettings["Authentication.PushServiceClientId"];
            var clientSecret = ConfigurationManager.AppSettings["Authentication.PushServiceClientSecret"];

            var clientCredential = new ClientCredential(clientId, clientSecret);

            var authorityUri = ConfigurationManager.AppSettings["Authentication.AuthorityUri"];
            var tenant = ConfigurationManager.AppSettings["Authentication.Tenant"];

            var authority = string.Concat(authorityUri, tenant);

            var authContext = new AuthenticationContext(authority);

            var authenticationResult = await authContext.AcquireTokenAsync(appIdUri, clientCredential);
            if (authenticationResult != null && !string.IsNullOrWhiteSpace(authenticationResult.AccessToken))
            {
                return authenticationResult.AccessToken;
            }

            return string.Empty;
        }
    }
}
