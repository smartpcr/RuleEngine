// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PipelineRuntime.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Engines
{
    using System;
    using System.Collections.Generic;

    public class PipelineRuntime
    {
        public string RunId { get; set; }
        public Type ContextType { get; set; }
        public PayloadScope Scope { get; set; }
        public Dictionary<string, object> Context { get; set; }
    }
}