// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FunctionExpression.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions.Functions
{
    using System.Linq.Expressions;

    public abstract class FunctionExpression
    {
        protected Expression Target { get; }
        protected FunctionName FuncName { get; }
        public string[] Args { get; }

        protected FunctionExpression(Expression target, FunctionName funcName, params string[] args)
        {
            Target = target;
            FuncName = funcName;
            Args = args;
        }

        public abstract Expression Build();
    }
}