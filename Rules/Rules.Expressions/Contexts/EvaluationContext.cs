// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EvaluationContext.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions.Contexts
{
    using System.Collections.Generic;
    using System.Threading;

    public class EvaluationContext
    {
        public static readonly AsyncLocal<string> CurrentRuleId = new AsyncLocal<string>();
        
        public Dictionary<string, object> Props { get; }

        public EvaluationContext()
        {
            Props=new Dictionary<string, object>();
        }
    }
}