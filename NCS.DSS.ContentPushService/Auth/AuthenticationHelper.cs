﻿using Microsoft.Extensions.Logging;
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

            log.LogInformation("Attempting to retrieve access token for following details: \nClientId:{ClientId}\nClient secret:{ClientSecret}\nAuthority Uri: {AuthorityUri}\nTenant: {Tenant}", clientId, clientSecret, authorityUri, tenant);
            var scopes = new[] { appIdUri + "/.default" }; 
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
