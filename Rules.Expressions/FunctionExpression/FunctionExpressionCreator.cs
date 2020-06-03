// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FunctionExpressionCreator.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions.FunctionExpression
{
    using System;
    using System.Linq.Expressions;

    public class FunctionExpressionCreator
    {
        public FunctionExpression Create(Expression target, FunctionName funcName, string arg)
        {
            switch (funcName)
            {
                case FunctionName.Average:
                case FunctionName.Count:
                case FunctionName.DistinctCount:
                case FunctionName.Max:
                case FunctionName.Min:
                case FunctionName.Sum:
                    return new AggregateExpression(target, funcName, arg);
                case FunctionName.Select:
                    return new SelectExpression(target, arg);
                case FunctionName.Ago:
                    return new AgoExpression(target, funcName, arg);
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public abstract class FunctionExpression
    {
        protected Expression Target { get; }
        protected FunctionName FuncName { get; }
        protected string FuncArg { get; }

        protected FunctionExpression(Expression target, FunctionName funcName, string funcArg)
        {
            Target = target;
            FuncName = funcName;
            FuncArg = funcArg;
        }

        public abstract MethodCallExpression Create();
    }
}