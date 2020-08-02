// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AllInCall.cs" company="Microsoft Corporation">
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

    public class AllInCall : OperatorExpression
    {
        public AllInCall(Expression leftExpression, Expression rightExpression) : base(leftExpression, rightExpression)
        {
            if (rightExpression.Type != typeof(string[]))
            {
                throw new InvalidOperationException($"right side {rightExpression} type should be string array");
            }

            if (leftExpression.Type != typeof(IEnumerable<string>) &&
                leftExpression.Type != typeof(List<string>) &&
                leftExpression.Type != typeof(string[]))
            {
                throw new InvalidCastException($"left side expression {leftExpression} have invalid type");
            }
        }

        public override Expression Create()
        {
            var stringParamExpr = Expression.Parameter(typeof(string), "s");
            var containsMethod = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Single(x => x.Name == "Contains" && x.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(string));
            var containsBody = Expression.Call(containsMethod, RightExpression, stringParamExpr);
            var predicateExpr = Expression.Lambda<Func<string, bool>>(containsBody, stringParamExpr);

            var allInExpression = Expression.Call(
                typeof(Enumerable),
                "All",
                new[] {typeof(string)},
                LeftExpression,
                predicateExpr);

            return allInExpression;
        }
    }
}