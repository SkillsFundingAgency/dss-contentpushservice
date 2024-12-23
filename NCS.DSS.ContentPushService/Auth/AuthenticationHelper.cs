using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

namespace NCS.DSS.ContentPushService.Auth
{
    public static class AuthenticationHelper
    {
        public static async Task<string> GetAccessToken(string clientUrl, ILogger log)
        {
            var clientId = Environment.GetEnvironmentVariable("Authentication.PushServiceClientId");
            var clientSecret = Environment.GetEnvironmentVariable("Authentication.PushServiceClientSecret");

            var authorityUri = Environment.GetEnvironmentVariable("Authentication.AuthorityUri");
            var tenant = Environment.GetEnvironmentVariable("Authentication.Tenant");

            var authority = string.Concat(authorityUri, tenant);

            var app = ConfidentialClientApplicationBuilder.Create(clientId)
            .WithClientSecret(clientSecret)
            .WithAuthority(new Uri(authority))
            .Build();

            log.LogInformation("Attempting to retrieve access token");
            var scopes = new[] { clientUrl + "/.default" }; 
            var authenticationResult = await app.AcquireTokenForClient(scopes).ExecuteAsync();

            if (authenticationResult != null && !string.IsNullOrWhiteSpace(authenticationResult.AccessToken))
            {
                log.LogInformation("Successfully retrieved access token");
                return authenticationResult.AccessToken;
            }

            log.LogWarning("Failed to retrieve access token");
            return string.Empty;
        }
    }
}
