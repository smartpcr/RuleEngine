// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ContainsCall.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions.Operators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    public class Contains : OperatorExpression
    {
        private const string MethodName = "Contains";

        public Contains(Expression leftExpression, Expression rightExpression) : base(leftExpression, rightExpression)
        {
        }

        public override Expression Create()
        {
            if (LeftExpression.Type == typeof(string))
            {
                var methodInfo = typeof(string).GetMethod(MethodName, new[] {typeof(string)});
                if (methodInfo == null) throw new Exception("Invalid method: " + MethodName + " for type string");
                return Expression.Call(LeftExpression, methodInfo, RightExpression);
            }

            var extensionType = typeof(Enumerable);
            var argumentTypes = new[] {typeof(IEnumerable<string>).GenericTypeArguments[0]};
            return Expression.Call(
                extensionType,
                MethodName,
                argumentTypes,
                LeftExpression,
                RightExpression);
        }
    }
}