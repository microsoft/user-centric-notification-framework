// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Notification.Contract
{
    using Newtonsoft.Json;
    using System.Collections.Generic;

    public class WebPushNotificationRegistrationEntity : BaseTableEntity
    {
        public string UserAlias { get; set; }

        public string EndPoint { get; set; }

        public string ExpirationTime { get; set; }

        [JsonIgnore]
        public Dictionary<string, string> Keys { get; set; }

        public string Auth { get; set; }

        public string P256DH { get; set; }

        // TODO:: Future
        // Store Browser Name
    }
}