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
        public FunctionExpression Create(Expression target, FunctionName funcName, params string[] args)
        {
            switch (funcName)
            {
                case FunctionName.Average:
                case FunctionName.Count:
                case FunctionName.DistinctCount:
                case FunctionName.Max:
                case FunctionName.Min:
                case FunctionName.Sum:
                    return new AggregateExpression(target, funcName, args);
                case FunctionName.Select:
                    return new SelectExpression(target, args);
                case FunctionName.Ago:
                    return new AgoExpression(target, funcName, args);
                case FunctionName.Where:
                    return new WhereExpression(target, funcName, args);
                case FunctionName.First:
                    return new FirstExpression(target, funcName, args);
                case FunctionName.Last:
                    return new LastExpression(target, funcName, args);
                case FunctionName.Traverse:
                    return new TraverseExpression(target, funcName, args);
                default:
                    throw new NotImplementedException();
            }
        }
    }

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