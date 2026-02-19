// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Notification.BL.Common.Interface
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;

    public interface IHttpHelper
    {
        /// <summary>
        /// Send Request to target REST endpoint
        /// </summary>
        /// <param name="method"></param>
        /// <param name="resourceUri"></param>
        /// <param name="targetUri"></param>
        /// <param name="content"></param>
        /// <param name="headers"></param>
        /// <param name="isTokenAttachedRequired"></param>
        /// <returns></returns>
        Task<HttpResponseMessage> SendRequestAsync(
            HttpMethod method,
            string resourceUri,
            string targetUri,
            string content = "",
            Dictionary<string, string> headers = null,
            bool isTokenAttachedRequired = true);
    }
}