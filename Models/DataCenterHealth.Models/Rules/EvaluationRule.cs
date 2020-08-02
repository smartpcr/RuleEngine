// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EvaluationRule.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models.Rules
{
    public class EvaluationRule : Rule
    {
        public string PluginName { get; set; }
        public string AssemblyName { get; set; }

        public EvaluationRule()
        {
            Type = RuleType.Plugin;
        }
    }
}