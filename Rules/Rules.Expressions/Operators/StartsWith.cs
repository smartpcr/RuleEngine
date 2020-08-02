// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StartsWithCall.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions.Operators
{
    using System;
    using System.Linq.Expressions;

    public class StartsWith : OperatorExpression
    {
        private const string methodName = "StartsWith";

        public StartsWith(Expression leftExpression, Expression rightExpression) : base(leftExpression, rightExpression)
        {
            if (leftExpression.Type != typeof(string) || rightExpression.Type != typeof(string))
            {
                throw new InvalidOperationException($"both left side and right side must be type string for method '{methodName}'");
            }
        }

        public override Expression Create()
        {
            var methodInfo = typeof(string).GetMethod(methodName, new[] {typeof(string)});
            if (methodInfo == null) throw new Exception("Invalid method: " + methodName + " for type string");
            return Expression.Call(LeftExpression, methodInfo, RightExpression);
        }
    }
}