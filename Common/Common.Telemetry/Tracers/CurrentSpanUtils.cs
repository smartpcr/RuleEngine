// <copyright file="CurrentSpanUtils.cs" company="OpenTelemetry Authors">
// Copyright 2018, OpenTelemetry Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

namespace Common.Telemetry.Tracers
{
    using System;
    using System.Threading;
    using OpenTelemetry.Trace;

    /// <summary>
    ///     Span utils for Logging-only SDK implementation.
    /// </summary>
    internal static class CurrentSpanUtils
    {
        private static readonly AsyncLocal<TelemetrySpan> AsyncLocalContext = new AsyncLocal<TelemetrySpan>();

        public static TelemetrySpan CurrentSpan => AsyncLocalContext.Value;

        public class LoggingScope : IDisposable
        {
            private readonly bool endSpan;
            private readonly TelemetrySpan origContext;
            private readonly TelemetrySpan span;

            public LoggingScope(TelemetrySpan span, bool endSpan = true)
            {
                this.span = span;
                this.endSpan = endSpan;
                origContext = AsyncLocalContext.Value;
                AsyncLocalContext.Value = span;
            }

            /// <inheritdoc />
            public void Dispose()
            {
                Logger.Log("Scope.Dispose");
                var current = AsyncLocalContext.Value;
                AsyncLocalContext.Value = origContext;

                if (current != origContext) Logger.Log("Scope.Dispose: current != this.origContext");

                if (endSpan) span.End();
            }
        }
    }
}