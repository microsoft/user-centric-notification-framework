// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Notification.Contract
{
    /// <summary>
    /// The EmailNotificationStatus class
    /// </summary>
    public class EmailNotificationStatus : BaseTableEntity
    {
        public string Status { get; set; }
    }
}