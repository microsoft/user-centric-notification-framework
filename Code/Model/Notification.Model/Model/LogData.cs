// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Notification.Model.Model
{
    using System.Collections.Generic;

    public class LogData
    {
        public LogType LogType { get; set; }

        public Dictionary<string, object> EventDetails { get; set; }

        public LogData()
        {
            EventDetails = new Dictionary<string, object>();
        }
    }
}