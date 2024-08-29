using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace NCS.DSS.ContentPushService.Auth
{
    public static class AuthenticationHelper
    {
        public static async Task<string> GetAccessToken(string appIdUri)
        {
            var clientId = Environment.GetEnvironmentVariable("Authentication.PushServiceClientId");
            var clientSecret = Environment.GetEnvironmentVariable("Authentication.PushServiceClientSecret");

            var clientCredential = new ClientCredential(clientId, clientSecret);

            var authorityUri = Environment.GetEnvironmentVariable("Authentication.AuthorityUri");
            var tenant = Environment.GetEnvironmentVariable("Authentication.Tenant");

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
