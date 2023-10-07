// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Notification.BL.PerformanceLogger.Helpers
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// starts & stops the timer and set some private properties used to log the custom metrics
    /// </summary>
    public class CustomPerformanceTracer : IDisposable
    {
        private readonly Stopwatch _stopwatch = new Stopwatch();

        public CustomPerformanceTracer()
        {
            _stopwatch.Start();
        }

        public void Dispose()
        {
            _stopwatch.Stop();
        }

        public TimeSpan Result
        {
            get { return _stopwatch.Elapsed; }
        }
    }
}