// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPipeline.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Engines
{
    using System.Collections.Generic;
    using System.Threading.Tasks.Dataflow;

    public interface IPipeline
    {
        List<IDataflowBlock> Activities { get; }
    }

    public class Pipeline : IPipeline
    {
        public List<IDataflowBlock> Activities { get; }
        
        public Pipeline(List<IDataflowBlock> activities)
        {
            Activities = activities;
        }

        
    }
}