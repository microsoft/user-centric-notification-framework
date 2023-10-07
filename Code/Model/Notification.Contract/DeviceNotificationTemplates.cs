// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Notification.Contract
{
    public class DeviceNotificationTemplates : BaseTableEntity
    {
        [JsonProperty("templateContent")]
        public string TemplateContent { get; set; }
    }
}
