// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Notification.BL.Common.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using Interface;
    using Microsoft.Extensions.Configuration;

    public class HttpHelper : IHttpHelper
    {
        private readonly IConfiguration _config;
        private readonly IAuthenticationHelper _authenticationHelper;
        private readonly HttpClient _client;

        public HttpHelper(IConfiguration config, HttpClient httpClient, IAuthenticationHelper authenticationHelper)
        {
            _config = config;
            _client = httpClient;
            _authenticationHelper = authenticationHelper;
        }

        /// <summary>
        /// Send Request to target REST endpoint
        /// </summary>
        /// <param name="method"></param>
        /// <param name="clientId"></param>
        /// <param name="clientKey"></param>
        /// <param name="authority"></param>
        /// <param name="resourceUri"></param>
        /// <param name="scope"></param>
        /// <param name="targetUri"></param>
        /// <param name="content"></param>
        /// <param name="headers"></param>
        /// <param name="isTokenAttachedRequired"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> SendRequestAsync(
            HttpMethod method,
            string clientId,
            string clientKey,
            string authority,
            string resourceUri,
            string scope,
            string targetUri,
            string content = "",
            Dictionary<string, string> headers = null,
            bool isTokenAttachedRequired = true)
        {
            if (isTokenAttachedRequired)
            {
                if (string.IsNullOrWhiteSpace(clientId))
                {
                    clientId = _config[Constant.IdentityProviderClientId];
                }
                if (string.IsNullOrWhiteSpace(clientKey))
                {
                    clientKey = _config[Constant.IdentityProviderAppKey];
                }
                if (string.IsNullOrWhiteSpace(authority))
                {
                    authority = _config[Constant.IdentityProviderAuthority];
                }
                if (string.IsNullOrWhiteSpace(resourceUri))
                {
                    resourceUri = _config[Constant.IdentityProviderResource];
                }
                if (string.IsNullOrWhiteSpace(scope))
                {
                    scope = "/.default";
                }

                // Get Access token
                var accessToken = (await _authenticationHelper.GetAccessToken(
                    clientId,
                    clientKey,
                    authority,
                    resourceUri,
                    scope)).AccessToken;

                _client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue(Constant.Bearer, accessToken);
            }

            // Create HttpRequestMessage
            var httpRequest = new HttpRequestMessage(method, new Uri(targetUri));
            if (!string.IsNullOrWhiteSpace(content))
            {
                httpRequest.Content = new StringContent(content, Encoding.UTF8, Constant.ApplicationJson);
            }

            // Add Http Headers
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    if (httpRequest.Headers.Contains(header.Key))
                    {
                        httpRequest.Headers.Remove(header.Key);
                    }
                    httpRequest.Headers.Add(header.Key, header.Value);
                }
            }
            HttpResponseMessage response = await _client.SendAsync(httpRequest);
            return response;
        }
    }
}