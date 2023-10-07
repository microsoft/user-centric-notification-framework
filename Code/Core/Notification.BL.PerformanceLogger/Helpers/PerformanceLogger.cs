// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Notification.BL.PerformanceLogger.Helpers
{
    using System;
    using Interfaces;

    public class PerformanceLogger : IPerformanceLogger
    {
        /// <summary>
        /// logs the total time taken to process each amex transaction through expense green channel automation in application insights custom metrics
        /// </summary>
        public IDisposable StartPerformanceLogger()
        {
            return new CustomPerformanceTracer();
        }
    }
}