// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OperationScope.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Common.Telemetry
{
    using System;
    using System.Diagnostics;
    using EnsureThat;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility;

    /// <summary>
    ///     A wrapper to app insights method TelemetryClient.StartOperation that will create
    ///     either RequestTelemetry or DependencyTelemetry based on if current call is root
    ///     This class is returned in using statement in order to emit metric when it's being disposed
    /// </summary>
    public sealed class OperationScope : IDisposable
    {
        public const string REQUEST_TELEMETRY_KEY = "request-id";
        private readonly TelemetryClient telemetryClient;
        private readonly IOperationHolder<RequestTelemetry> requestOperation;

        public OperationScope(Activity activity, TelemetryClient telemetryClient = null)
        {
            Ensure.That(activity).IsNotNull();
            this.telemetryClient = telemetryClient;
            if (this.telemetryClient != null)
                requestOperation = this.telemetryClient.StartOperation<RequestTelemetry>(activity);
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }


        /// <summary>
        ///     A helper to create <see cref="IDisposable" /> from caller class
        ///     Example:
        ///     using (_metrics.StartOperation("methodName")) {}
        ///     using (_metrics.StartOperation("methodName", parentOperationId)) {}
        ///     if parentId is not passed in, it checks static instance AsyncLocal to find current activity,
        ///     which is stored as local copy for each call (ExecutionContext) within a process.
        ///     When parentId is not found, it assumes current call is the root
        /// </summary>
        public static IDisposable StartOperation(
            string parentOperationId,
            string operationName,
            TelemetryClient appInsights)
        {
            var activity = new Activity(operationName);

            if (!string.IsNullOrEmpty(parentOperationId))
                activity.SetParentId(parentOperationId);
            else
                activity.SetIdFormat(ActivityIdFormat.W3C);

            return new OperationScope(activity, appInsights);
        }

        private void ReleaseUnmanagedResources()
        {
            if (requestOperation != null)
            {
                telemetryClient?.StopOperation(requestOperation);
                requestOperation?.Dispose();
            }
        }

        ~OperationScope()
        {
            ReleaseUnmanagedResources();
        }
    }
}