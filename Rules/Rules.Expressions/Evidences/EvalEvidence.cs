// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EvalEvidence.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions.Evidences
{
    using System;
    using Newtonsoft.Json.Linq;

    public class EvalEvidence
    {
        public LeafExpression Expression { get; set; }
        public Type LeftType { get; set; }
        public Type RightType { get; set; }
        public JToken Actual { get; set; }
        public JToken Expected { get; set; }
        public double Score { get; set; }
    }
}