// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Notification.BL.Common.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Extension;
    using Interface;
    using Microsoft.Extensions.Logging;
    using Microsoft.Identity.Client;
    using Model.Model;

    public class AuthenticationHelper : IAuthenticationHelper
    {
        private readonly ILogger _logger;

        public AuthenticationHelper(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Get the Access Token
        /// </summary>
        /// <param name="clientId">Client Id</param>
        /// <param name="appKey">Client Secret</param>
        /// <param name="authority">Authority</param>
        /// <param name="resource">Resource Uri</param>
        /// <param name="scope">Resource Uri</param>
        /// <returns>AuthenticationResult token</returns>
        public async Task<AuthenticationResult> GetAccessToken(string clientId, string appKey, string authority, string resource, string scope)
        {
            LogData logData = new LogData()
            {
                EventDetails = new Dictionary<string, object>() {
                    { Constant.BusinessProcessName, "Authentication - Get AccessToken" },
                    { Constant.ActionUri, "GetAccessToken" },
                    { Constant.ComponentType, ComponentType.WorkflowComponent }
                }
            };

            // Get an access token from Identity Provider using client credentials.
            // If the attempt to get a token fails because the server is unavailable, retry twice after 3 seconds each.

            AuthenticationResult result = null;
            var retryCount = 0;
            bool retry;

            do
            {
                retry = false;
                try
                {
                    // MSAL includes an in-memory cache, so this call will only send a message to the server if the cached token is expired.
                    IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.
                    Create(clientId).
                    WithClientSecret(appKey).
                    WithAuthority(new Uri(authority)).
                    Build();
                    var scopes = new[] { resource + scope };
                    result = await app.AcquireTokenForClient(scopes).ExecuteAsync();
                }
                catch (MsalServiceException ex)
                {
                    if (ex.ErrorCode == "temporarily_unavailable")
                    {
                        retry = true;
                        retryCount++;
                        Thread.Sleep(3000);
                    }

                    logData.EventDetails.Modify(Constant.AppAction, "Authentication - Get AccessToken - Failed - Exception");
                    using (_logger.BeginScope(logData.EventDetails))
                    {
                        _logger.LogError(new EventId((int)EventIds.GenericError),
                            ex, $"An error occurred while acquiring a token\nTime: {DateTime.Now.ToString(CultureInfo.InvariantCulture)}\nError: {ex}\nRetry: {retry.ToString()}\n");
                    }
                }
            } while (retry && (retryCount < 3));

            return result;
        }
    }
}