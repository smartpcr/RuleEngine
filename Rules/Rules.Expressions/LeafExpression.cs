// --------------------------------------------------------------------------------------------------------------------
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
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using OperatorExpression;

    public class LeafExpression : IConditionExpression
    {
        [JsonProperty(Required = Required.Always)]
        public string Left { get; set; }

        [JsonProperty(Required = Required.Always)]
        [JsonConverter(typeof(StringEnumConverter))]
        public Operator Operator { get; set; }

        public string Right { get; set; }

        public bool RightSideIsExpression { get; set; }

        public string[] OperatorArgs { get; set; }

        public Expression Process(ParameterExpression ctxExpression, Type parameterType)
        {
            var leftExpression = ctxExpression.BuildExpression(Left);
            leftExpression = leftExpression.AddToStringWithEnumType().AddValueWithNullableNumberType();
            var leftSideType = leftExpression.Type;
            Expression rightExpression;
            if (RightSideIsExpression)
            {
                rightExpression = ctxExpression.BuildExpression(Right);
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
                case Expressions.Operator.NotEquals:
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
                    generatedExpression = new ContainsCall(leftExpression, rightExpression).Create();
                    break;
                case Operator.NotContains:
                    generatedExpression = Expression.Not(new ContainsCall(leftExpression, rightExpression).Create());
                    break;
                case Operator.ContainsAll:
                    generatedExpression = new ContainsAllCall(leftExpression, rightExpression).Create();
                    break;
                case Operator.NotContainsAll:
                    generatedExpression = Expression.Not(new ContainsAllCall(leftExpression, rightExpression).Create());
                    break;
                case Operator.StartsWith:
                    generatedExpression = new StartsWithCall(leftExpression, rightExpression).Create();
                    break;
                case Operator.NotStartsWith:
                    generatedExpression = Expression.Not(new StartsWithCall(leftExpression, rightExpression).Create());
                    break;
                case Operator.In:
                    generatedExpression = new InCall(leftExpression, rightExpression).Create();
                    break;
                case Operator.NotIn:
                    generatedExpression = Expression.Not(new InCall(leftExpression, rightExpression).Create());
                    break;
                case Operator.AllIn:
                    generatedExpression = new AllInCall(leftExpression, rightExpression).Create();
                    break;
                case Operator.NotAllIn:
                    generatedExpression = Expression.Not(new AllInCall(leftExpression, rightExpression).Create());
                    break;
                case Operator.AnyIn:
                    generatedExpression = new AnyInCall(leftExpression, rightExpression).Create();
                    break;
                case Operator.NotAnyIn:
                    generatedExpression = Expression.Not(new AnyInCall(leftExpression, rightExpression).Create());
                    break;
                case Operator.IsNull:
                    generatedExpression = Expression.Equal(leftExpression, rightExpression);
                    break;
                case Operator.NotIsNull:
                    generatedExpression = Expression.Not(Expression.Equal(leftExpression, rightExpression));
                    break;
                case Operator.IsEmpty:
                    generatedExpression = new IsEmptyCall(leftExpression, rightExpression).Create();
                    break;
                case Operator.NotIsEmpty:
                    generatedExpression = Expression.Not(new IsEmptyCall(leftExpression, rightExpression).Create());
                    break;
                case Operator.DiffWithinPct:
                    generatedExpression = new DiffWithinPctCall(leftExpression, rightExpression, OperatorArgs).Create();
                    break;
                case Operator.AllInRangePct:
                    generatedExpression = new AllInRangeCall(leftExpression, rightExpression, OperatorArgs).Create();
                    break;
                default:
                    throw new NotSupportedException($"operation {Operator} is not supported");
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
                    return Expression.Constant(new decimal[]{min, max}, typeof(decimal[]));
                default:
                    return Expression.Constant(leftSideType.ConvertValue(Right));
            }
        }
    }
}