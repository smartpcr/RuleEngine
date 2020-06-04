// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TreeNode.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions.Tests.Contexts
{
    public class TreeNode
    {
        public string Id { get; set; }
        public TreeNode Parent { get; set; }
    }
}