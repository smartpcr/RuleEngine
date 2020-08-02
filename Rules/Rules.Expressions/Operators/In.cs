﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InCall.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions.Operators
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;

    public class In : OperatorExpression
    {
        public In(Expression leftExpression, Expression rightExpression) : base(leftExpression, rightExpression)
        {
            if (rightExpression.Type != typeof(string[]))
            {
                throw new InvalidOperationException($"right side {rightExpression} type should be string array");
            }
            if (leftExpression.Type == typeof(string) ||
                leftExpression.Type.IsEnum ||
                Nullable.GetUnderlyingType(leftExpression.Type) != null){}
            else
            {
                throw new InvalidCastException($"left side {leftExpression} doesn't have correct type");
            }
        }

        public override Expression Create()
        {
            return Expression.Call(
                typeof(Enumerable),
                "Contains",
                new[] {typeof(string)},
                RightExpression,
                LeftExpression);
        }
    }
}