// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Notification.Contract
{
    using System.Collections.Generic;

    public class DeviceRegistration
    {
        public string Platform { get; set; }
        public string Handle { get; set; }
        public List<string> Tags { get; set; }
        public string Id { get; set; }
        public string NotificationHubName { get; set; }
    }
}