using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using NCS.DSS.ContentPushService.Models;
using NCS.DSS.ContentPushService.PushService;

namespace NCS.DSS.ContentPushService.Auth
{
    public static class AuthenticationHelper
    {
        public static async Task<string> GetAccessToken(string appIdUri, ILogger<MessagePushService> log, IOptions<ContentPushServiceConfigurationSettings> configOptions)
        {
            log.LogInformation($"Function {nameof(GetAccessToken)} was invoked");

            var config = configOptions.Value;
            
            var clientId = config.AuthenticationPushServiceClientId;
            var clientSecret = config.AuthenticationPushServiceClientSecret;
            var authorityUri = config.AuthenticationAuthorityUri;
            var tenant = config.AuthenticationTenant;

            var authority = string.Concat(authorityUri, tenant);

            var app = ConfidentialClientApplicationBuilder.Create(clientId)
            .WithClientSecret(clientSecret)
            .WithAuthority(new Uri(authority))
            .Build();

            var scopes = new[] { appIdUri + "/.default" };
            AuthenticationResult authenticationResult;

            log.LogInformation($"Attempting to generate access token");

            try
            {
                authenticationResult = await app.AcquireTokenForClient(scopes).ExecuteAsync();
            }
            catch (Exception ex)
            {
                log.LogWarning($"Failed to retrieve access token; returning empty string. Exception: {ex}");
                return string.Empty;
            }

            if (authenticationResult != null && !string.IsNullOrWhiteSpace(authenticationResult.AccessToken))
            {
                log.LogInformation("Successfully retrieved access token");
                return authenticationResult.AccessToken;
            }

            log.LogWarning($"Failed to retrieve access token; returning empty string");
            return string.Empty;
        }
    }
}
