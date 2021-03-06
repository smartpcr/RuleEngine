﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Rule.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Models.Rules
{

    public class Rule : TrackableEntity
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public string WhenExpression { get; set; }
        public string IfExpression { get; set; }
    }
}