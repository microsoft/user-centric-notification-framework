// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Azure.Data.Tables;
using Newtonsoft.Json;

namespace Notification.Contract
{
    /// <summary>
    /// The BaseTableEntity class
    /// </summary>
    public class BaseTableEntity : ITableEntity
    {
        public BaseTableEntity()
        { }

        public BaseTableEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }

        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }

        [JsonIgnore]
        public global::Azure.ETag ETag { get; set; }
    }
}
