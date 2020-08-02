// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ContainsAllCall.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions.OperatorExpression
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    public class ContainsAllCall : OperatorExpression
    {
        private const string methodName = "ContainsAll";

        public ContainsAllCall(Expression leftExpression, Expression rightExpression) : base(leftExpression, rightExpression)
        {
            if (leftExpression.Type == typeof(IEnumerable<string>) ||
                leftExpression.Type == typeof(List<string>) ||
                leftExpression.Type == typeof(string[])){}
            else
            {
                throw new InvalidOperationException($"left side type: '{leftExpression}' is not supported for method {methodName}");
            }

            if (rightExpression.Type != typeof(string[]))
            {
                throw new InvalidOperationException($"left side type: '{rightExpression}' is not supported for method {methodName}");
            }
        }
        
        public override Expression Create()
        {
            var stringParamExpr = Expression.Parameter(typeof(string), "s");
            var containsMethod = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Single(x => x.Name == "Contains" && x.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(string));
            var containsBody = Expression.Call(containsMethod, LeftExpression, stringParamExpr);
            var predicateExpr = Expression.Lambda<Func<string, bool>>(containsBody, stringParamExpr);

            var allInExpression = Expression.Call(
                typeof(Enumerable),
                "All",
                new[] {typeof(string)},
                RightExpression,
                predicateExpr);

            return allInExpression;
        }
    }
}