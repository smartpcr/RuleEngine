// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IAppTelemetry.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Common.Telemetry
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public interface IAppTelemetry
    {
        string OperationId { get; }

        void RecordMetric(string name, long value, params (string key, string value)[] dimensions);

        IDisposable StartOperation(object caller, string parentOperationId = null,
            [CallerMemberName] string callerMember = "");

        void TrackEvent(string eventName, IDictionary<string, string> properties);
    }
}