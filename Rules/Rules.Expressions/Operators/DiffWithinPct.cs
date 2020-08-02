// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DiffWithinPctCall.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions.Operators
{
    using System;
    using System.Linq.Expressions;
    using Helpers;

    public class DiffWithinPct : OperatorExpression
    {
        private readonly decimal threshold;

        public DiffWithinPct(Expression leftExpression, Expression rightExpression, params string[] operatorArgs) : base(leftExpression, rightExpression)
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
            ParameterExpression isWithinRange = Expression.Variable(typeof(bool), "isWithinRange");
            ParameterExpression pctDifference = Expression.Variable(typeof(decimal), "pctDifference");
            var ifelse = Expression.IfThenElse(
                Expression.GreaterThan(over, Expression.Convert(Expression.Constant(0), typeof(decimal))),
                Expression.Block(
                    Expression.Assign(pctDifference,
                        Expression.Multiply(
                            Expression.Divide(abs, over),
                            Expression.Convert(Expression.Constant(100), typeof(decimal)))),
                    Expression.Assign(isWithinRange, Expression.LessThanOrEqual(pctDifference, Expression.Constant(threshold)))),
                Expression.Assign(isWithinRange, Expression.Constant(false, typeof(bool))));
            var block = Expression.Block(
                new[] {pctDifference, isWithinRange},
                ifelse,
                isWithinRange);

            return block;
        }
    }
}