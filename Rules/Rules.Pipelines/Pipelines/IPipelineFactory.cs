// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPipelineFactory.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Validations.Pipelines
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks.Dataflow;
    using DataCenterHealth.Models.Rules;

    public interface IPipelineFactory
    {
        IList<(PipelineActivityType activityType, IDataflowBlock block)> CreateJsonRulePipeline(
            PipelineExecutionContext context, CancellationToken cancellationToken);

        IList<(PipelineActivityType activityType, IDataflowBlock block)> CreateCodeRulePipeline(
            PipelineExecutionContext context,
            List<CodeRule> codeRules,
            CancellationToken cancellationToken);

        IList<(PipelineActivityType activityType, IDataflowBlock block)> CreateDataCenterPipeline(
            PipelineExecutionContext context,
            CancellationToken cancellationToken);
    }
}