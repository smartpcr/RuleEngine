// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DiffExpression.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions.OperatorExpression
{
    using System;
    using System.Linq.Expressions;

    public class DiffWithinPctCall : OperatorExpression
    {
        private readonly decimal threshold;
        
        public DiffWithinPctCall(Expression leftExpression, Expression rightExpression, params string[] operatorArgs) : base(leftExpression, rightExpression)
        {
            if (!leftExpression.Type.IsNumericType() || !rightExpression.Type.IsNumericType())
            {
                throw new InvalidOperationException($"Only numeric type is allowed on both sides of this operator. left: {leftExpression.Type}', right: '{rightExpression.Type}'");
            }

            if (operatorArgs == null || operatorArgs.Length != 1)
            {
                throw new InvalidOperationException($"Operator {GetType().Name} requires one argument");
            }

            threshold = decimal.Parse(operatorArgs[0]);
        }

        public override Expression Create()
        {
            var diff = Expression.Subtract(LeftExpression, RightExpression);
            var converted = Expression.Convert(diff, typeof(decimal));
            var absMethod = typeof(Math).GetMethod("Abs", new[] {typeof(decimal)});
            if (absMethod == null)
            {
                throw new InvalidOperationException("unable to find Abs method");
            }
            
            var abs = Expression.Call(null, absMethod, converted);
            var over = Expression.Convert(RightExpression, typeof(decimal));
            var pct = Expression.Multiply(Expression.Divide(abs, over), Expression.Convert(Expression.Constant(100), typeof(decimal)));
            return Expression.LessThanOrEqual(pct, Expression.Constant(threshold));
        }
    }
}