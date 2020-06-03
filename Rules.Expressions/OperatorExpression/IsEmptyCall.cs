// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IsEmptyCall.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions.OperatorExpression
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;

    public class IsEmptyCall : OperatorExpression
    {
        public IsEmptyCall(Expression leftExpression, Expression rightExpression) : base(leftExpression, rightExpression)
        {
        }

        public override Expression Create()
        {
            var isNull = Expression.Equal(LeftExpression, Expression.Constant(null, LeftExpression.Type));
            var anyCheck = Expression.Call(
                typeof(Enumerable),
                "Any",
                LeftExpression.Type.IsArray
                    ? new[] {LeftExpression.Type.GetGenericArguments()[0]}
                    : new[] {LeftExpression.Type.GenericTypeArguments[0]},
                LeftExpression);
            var isEmpty = Expression.Not(Expression.IsTrue(anyCheck));
            return Expression.OrElse(isNull, isEmpty);
        }
    }
}