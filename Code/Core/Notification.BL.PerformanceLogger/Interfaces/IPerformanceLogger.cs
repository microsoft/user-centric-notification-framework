// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Notification.BL.PerformanceLogger.Interfaces
{
    using System;

    public interface IPerformanceLogger
    {
        IDisposable StartPerformanceLogger();
    }
}