// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Notification.BL.Common.Interface
{
    using System.Threading.Tasks;
    using Microsoft.Identity.Client;

    public interface IAuthenticationHelper
    {
        /// <summary>
        /// Get the Access Token
        /// </summary>
        /// <param name="clientId">Client Id</param>
        /// <param name="appKey">Client Secret</param>
        /// <param name="authority">Authority</param>
        /// <param name="resource">Resource Uri</param>
        /// <param name="scope">Resource Uri</param>
        /// <returns>AuthenticationResult token</returns>
        Task<AuthenticationResult> GetAccessToken(string clientId, string appKey, string authority, string resource, string scope);
    }
}