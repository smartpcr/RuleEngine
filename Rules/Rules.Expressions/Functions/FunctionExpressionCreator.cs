// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FunctionExpressionCreator.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions.Functions
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
                    return new Aggregate(target, funcName, args);
                case FunctionName.Select:
                    return new Select(target, args);
                case FunctionName.SelectMany:
                    return new SelectMany(target, args);
                case FunctionName.Ago:
                    return new Ago(target, funcName, args);
                case FunctionName.Where:
                    return new Where(target, funcName, args);
                case FunctionName.First:
                case FunctionName.Last:
                    return new FirstOrLast(target, funcName, args);
                case FunctionName.OrderBy:
                case FunctionName.OrderByDesc:
                    return new OrderBy(target, funcName, args);
                case FunctionName.Traverse:
                    return new Traverse(target, funcName, args);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}