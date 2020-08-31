﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LeafExpression.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Evaluators;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Rules.Expressions.Helpers;
    using Rules.Expressions.Operators;

    public class LeafExpression : IConditionExpression
    {
        [JsonProperty(Required = Required.Always)]
        public string Left { get; set; }

        [JsonProperty(Required = Required.Always)]
        [JsonConverter(typeof(StringEnumConverter), true)]
        public Operator Operator { get; set; }

        public string Right { get; set; }

        public bool RightSideIsExpression { get; set; }

        public string[] OperatorArgs { get; set; }

        public Expression Process(ParameterExpression ctxExpression, Type parameterType)
        {
            var leftExpression = ctxExpression.EvaluateExpression(Left, false);
            leftExpression = leftExpression.AddToStringWithEnumType();
            if (Operator != Operator.IsNull && Operator != Operator.NotIsNull)
            {
                leftExpression = leftExpression.AddValueWithNullableNumberType();
            }
            var leftSideType = leftExpression.Type;
            Expression rightExpression;
            if (RightSideIsExpression)
            {
                rightExpression = ctxExpression.EvaluateExpression(Right);
                rightExpression = rightExpression.AddToStringWithEnumType().AddValueWithNullableNumberType();
            }
            else
            {
                rightExpression = GetRightConstantExpression(leftSideType);
            }

            Expression generatedExpression;
            switch (Operator)
            {
                case Operator.Equals:
                    generatedExpression = Expression.Equal(leftExpression, rightExpression);
                    break;
                case Operator.NotEquals:
                    generatedExpression = Expression.Not(Expression.Equal(leftExpression, rightExpression));
                    break;
                case Operator.GreaterThan:
                    generatedExpression = leftExpression.Type == typeof(DateTime)
                        ? Expression.MakeBinary(ExpressionType.GreaterThan, leftExpression, rightExpression)
                        : Expression.GreaterThan(leftExpression, rightExpression);
                    break;
                case Operator.GreaterOrEqual:
                    generatedExpression = leftExpression.Type == typeof(DateTime)
                        ? Expression.MakeBinary(ExpressionType.GreaterThanOrEqual, leftExpression, rightExpression)
                        : Expression.GreaterThanOrEqual(leftExpression, rightExpression);
                    break;
                case Operator.LessThan:
                    generatedExpression = leftExpression.Type == typeof(DateTime)
                        ? Expression.MakeBinary(ExpressionType.LessThan, leftExpression, rightExpression)
                        : Expression.LessThan(leftExpression, rightExpression);
                    break;
                case Operator.LessOrEqual:
                    generatedExpression = leftExpression.Type == typeof(DateTime)
                        ? Expression.MakeBinary(ExpressionType.LessThanOrEqual, leftExpression, rightExpression)
                        : Expression.LessThanOrEqual(leftExpression, rightExpression);
                    break;
                case Operator.Contains:
                    generatedExpression = new Contains(leftExpression, rightExpression).Create();
                    break;
                case Operator.NotContains:
                    generatedExpression = Expression.Not(new Contains(leftExpression, rightExpression).Create());
                    break;
                case Operator.ContainsAll:
                    generatedExpression = new ContainsAll(leftExpression, rightExpression).Create();
                    break;
                case Operator.NotContainsAll:
                    generatedExpression = Expression.Not(new ContainsAll(leftExpression, rightExpression).Create());
                    break;
                case Operator.StartsWith:
                    generatedExpression = new StartsWith(leftExpression, rightExpression).Create();
                    break;
                case Operator.NotStartsWith:
                    generatedExpression = Expression.Not(new StartsWith(leftExpression, rightExpression).Create());
                    break;
                case Operator.In:
                    generatedExpression = new In(leftExpression, rightExpression).Create();
                    break;
                case Operator.NotIn:
                    generatedExpression = Expression.Not(new In(leftExpression, rightExpression).Create());
                    break;
                case Operator.AllIn:
                    generatedExpression = new AllIn(leftExpression, rightExpression).Create();
                    break;
                case Operator.NotAllIn:
                    generatedExpression = Expression.Not(new AllIn(leftExpression, rightExpression).Create());
                    break;
                case Operator.AnyIn:
                    generatedExpression = new AnyIn(leftExpression, rightExpression).Create();
                    break;
                case Operator.NotAnyIn:
                    generatedExpression = Expression.Not(new AnyIn(leftExpression, rightExpression).Create());
                    break;
                case Operator.IsNull:
                    if (leftSideType.IsPrimitiveType() && Nullable.GetUnderlyingType(leftSideType) != null)
                    {
                        rightExpression = Expression.Constant(null, leftSideType);
                    }
                    generatedExpression = Expression.Equal(leftExpression, rightExpression);
                    break;
                case Operator.NotIsNull:
                    if (leftSideType.IsPrimitiveType() && Nullable.GetUnderlyingType(leftSideType) != null)
                    {
                        rightExpression = Expression.Constant(null, leftSideType);
                    }
                    generatedExpression = Expression.Not(Expression.Equal(leftExpression, rightExpression));
                    break;
                case Operator.IsEmpty:
                    generatedExpression = new IsEmpty(leftExpression, rightExpression).Create();
                    break;
                case Operator.NotIsEmpty:
                    generatedExpression = Expression.Not(new IsEmpty(leftExpression, rightExpression).Create());
                    break;
                case Operator.DiffWithinPct:
                    generatedExpression = new DiffWithinPct(leftExpression, rightExpression, OperatorArgs).Create();
                    break;
                case Operator.AllInRangePct:
                    generatedExpression = new AllInRange(leftExpression, rightExpression, OperatorArgs).Create();
                    break;
                default:
                    throw new NotSupportedException($"operation {Operator} is not supported");
            }

            if (Operator == Operator.IsNull || Operator == Operator.NotIsNull)
            {
                return generatedExpression;
            }

            return leftExpression.AddNotNullCheck(out var nullCheckExpression)
                ? Expression.AndAlso(nullCheckExpression, generatedExpression)
                : generatedExpression;
        }

        private Expression GetRightConstantExpression(Type leftSideType)
        {
            switch (Operator)
            {
                case Operator.ContainsAll:
                case Operator.NotContainsAll:
                case Operator.AllIn:
                case Operator.NotAllIn:
                case Operator.AnyIn:
                case Operator.NotAnyIn:
                    var stringArray = Right.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim()).ToArray();
                    return Expression.Constant(stringArray, typeof(string[]));
                case Operator.In:
                case Operator.NotIn:
                    var argumentValues = Right.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim()).ToArray();
                    return Expression.NewArrayInit(typeof(string), argumentValues.Select(Expression.Constant));
                case Operator.Contains:
                case Operator.NotContains:
                    return Expression.Constant(typeof(string).ConvertValue(Right));
                case Operator.IsNull:
                case Operator.NotIsNull:
                case Operator.IsEmpty:
                case Operator.NotIsEmpty:
                    return Expression.Constant(null, typeof(object));
                case Operator.AllInRangePct:
                    var decimalArray = Right.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim()).Select(decimal.Parse);
                    var min = decimalArray.Min();
                    var max = decimalArray.Max();
                    return Expression.Constant(new[]{min, max}, typeof(decimal[]));
                default:
                    return Expression.Constant(leftSideType.ConvertValue(Right));
            }
        }
    }
}