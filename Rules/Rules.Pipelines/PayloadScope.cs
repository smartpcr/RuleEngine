// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PayloadScope.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Engines
{
    using System.Collections.Generic;

    public class PayloadScope
    {
        public string DcName { get; set; }
        public List<string> RuleIds { get; set; }
        public List<string> PayloadIds { get; set; }
    }
}