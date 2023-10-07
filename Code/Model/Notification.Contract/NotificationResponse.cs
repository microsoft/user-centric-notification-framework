// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Notification.Contract
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Newtonsoft.Json;

    public class NotificationResponse
    {
        [JsonProperty("tenantIdentifier")]
        public string TenantIdentifier { get; set; }

        [JsonProperty("actionResult")]
        public bool ActionResult { get; set; }

        [JsonProperty("e2EErrorInformation")]
        public E2EErrorInformation E2EErrorInformation { get; set; }

        [JsonProperty("displayMessage")]
        public string DisplayMessage { get; set; }

        [JsonProperty("telemetry")]
        public Telemetry Telemetry { get; set; }

        [JsonProperty("sequenceNumber")]
        public long SequenceNumber { get; set; }
    }

    public class E2EErrorInformation
    {
        [JsonProperty("errorMessages")]
        public List<string> ErrorMessages { get; set; }

        [JsonProperty("errorType")]
        public ResponseErrorType ErrorType { get; set; }

        [JsonProperty("retryInterval")]
        public long RetryInterval { get; set; }
    }

    public enum ResponseErrorType
    {
        /// <summary>
        /// Intended Error.
        /// </summary>
        [EnumMember]
        IntendedError = 1,

        /// <summary>
        /// Unintended Non-Transient Error.
        /// </summary>
        [EnumMember]
        UnintendedError = 2,

        /// <summary>
        /// Unintended Transient Error.
        /// </summary>
        [EnumMember]
        UnintendedTransientError = 3
    }
}