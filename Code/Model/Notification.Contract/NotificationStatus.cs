// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Notification.Contract
{
    using Newtonsoft.Json;

    public class NotificationStatus : BaseTableEntity
    {
        [JsonProperty("tenantIdentifier")]
        public string TenantIdentifier { get; set; }

        [JsonProperty("actionResult")]
        public bool ActionResult { get; set; }

        [JsonProperty("messageId")]
        public string MessageId { get; set; }

        [JsonProperty("xcv")]
        public string Xcv { get; set; }

        [JsonProperty("sequenceNumber")]
        public long SequenceNumber { get; set; }
    }
}