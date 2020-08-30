// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Rule.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Models.Rules
{
    using System.Collections.Generic;

    public abstract class Rule : TrackableEntity
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public List<string> Owners { get; set; }
        public List<string> Contributors { get; set; }
        public string Description { get; set; }
        public string WhenExpression { get; set; }
        public string IfExpression { get; set; }
    }
}