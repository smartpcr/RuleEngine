// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EvaluationEvidence.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions.Contexts
{
    public class EvaluationEvidence
    {
        public string Path { get; set; }
        public string Actual { get; set; }
        public string Expected { get; set; }
        public string Error { get; set; }
    }
}