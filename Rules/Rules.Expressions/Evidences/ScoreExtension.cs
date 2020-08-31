// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Scoring.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions.Evidences
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Evaluators;
    using Helpers;
    using Newtonsoft.Json.Linq;

    public static class ScoreExtension
    {
        private const double TOLERANCE = 0.0001;

        public static Func<T, double> GetScore<T>(this LeafExpression leafExpression)
        {
            var ctxExpression = Expression.Parameter(typeof(T), "ctx");
            var targetExpression = ctxExpression.EvaluateExpression(
                leafExpression.Left,
                leafExpression.Operator != Operator.IsNull && leafExpression.Operator != Operator.NotIsNull);

            if (targetExpression.Type == typeof(int))
            {
                var lambda = Expression.Lambda<Func<T, int>>(targetExpression, ctxExpression);
                var getValue = lambda.Compile();
                var expected = (int) Convert.ChangeType(leafExpression.Right, typeof(int));

                double getScore(T t)
                {
                    var actual = getValue(t);
                    switch (leafExpression.Operator)
                    {
                        case Operator.GreaterThan:
                            return actual > expected ? 1.0 : (double) actual / expected;
                        case Operator.GreaterOrEqual:
                            return actual >= expected ? 1.0 : (double) actual / expected;
                        case Operator.LessThan:
                            return actual < expected ? 1.0 : (double) (actual - expected) / expected;
                        case Operator.LessOrEqual:
                            return actual <= expected ? 1.0 : (double) (actual - expected) / expected;
                        default:
                            return actual == expected ? 1.0 : 0.0;
                    }
                }

                return getScore;
            }

            if (targetExpression.Type == typeof(double))
            {
                var lambda = Expression.Lambda<Func<T, double>>(targetExpression, ctxExpression);
                var getValue = lambda.Compile();
                var expected = (double) Convert.ChangeType(leafExpression.Right, typeof(double));

                double getScore(T t)
                {
                    var actual = getValue(t);
                    switch (leafExpression.Operator)
                    {
                        case Operator.GreaterThan:
                            return actual > expected ? 1.0 : actual / expected;
                        case Operator.GreaterOrEqual:
                            return actual >= expected ? 1.0 : actual / expected;
                        case Operator.LessThan:
                            return actual < expected ? 1.0 : (actual - expected) / expected;
                        case Operator.LessOrEqual:
                            return actual <= expected ? 1.0 : (actual - expected) / expected;
                        default:
                            return Math.Abs(actual - expected) < 0.001 ? 1.0 : 0.0;
                    }
                }

                return getScore;
            }

            if (targetExpression.Type == typeof(decimal))
            {
                var lambda = Expression.Lambda<Func<T, decimal>>(targetExpression, ctxExpression);
                var getValue = lambda.Compile();
                var expected = (decimal) Convert.ChangeType(leafExpression.Right, typeof(decimal));

                double getScore(T t)
                {
                    var actual = getValue(t);
                    switch (leafExpression.Operator)
                    {
                        case Operator.GreaterThan:
                            return actual > expected ? 1.0 : (double) actual / (double) expected;
                        case Operator.GreaterOrEqual:
                            return actual >= expected ? 1.0 : (double) actual / (double) expected;
                        case Operator.LessThan:
                            return actual < expected ? 1.0 : (double) (actual - expected) / (double) expected;
                        case Operator.LessOrEqual:
                            return actual <= expected ? 1.0 : (double) (actual - expected) / (double) expected;
                        default:
                            return actual == expected ? 1.0 : 0.0;
                    }
                }

                return getScore;
            }

            if (targetExpression.Type == typeof(string))
            {
                var lambda = Expression.Lambda<Func<T, string>>(targetExpression, ctxExpression);
                var getValue = lambda.Compile();
                var expectedString = leafExpression.Right;
                var expectedArray = leafExpression.Right.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim()).ToArray();

                double getScore(T t)
                {
                    var actual = getValue(t);
                    switch (leafExpression.Operator)
                    {
                        case Operator.StartsWith:
                            if (expectedString is string expected1) return actual.StartsWith(expected1) ? 1.0 : 0.0;
                            return 0.0;
                        case Operator.NotStartsWith:
                            if (expectedString is string expected2) return !actual.StartsWith(expected2) ? 1.0 : 0.0;
                            return 0.0;
                        case Operator.Contains:
                            if (expectedString is string expected3) return actual.Contains(expected3) ? 1.0 : 0.0;
                            return 0.0;
                        case Operator.NotContains:
                            if (expectedString is string expected4) return !actual.Contains(expected4) ? 1.0 : 0.0;
                            return 0.0;
                        case Operator.In:
                            return expectedArray.Contains(actual) ? 1.0 : 0.0;
                        case Operator.NotIn:
                            return expectedArray.Contains(actual) ? 0.0 : 1.0;
                    }

                    if (expectedString is string expected) return actual == expected ? 1.0 : 0.0;
                    return 0.0;
                }

                return getScore;
            }

            if (targetExpression.Type == typeof(bool))
            {
                var lambda = Expression.Lambda<Func<T, bool>>(targetExpression, ctxExpression);
                var getValue = lambda.Compile();
                var expected = (bool) Convert.ChangeType(leafExpression.Right, typeof(bool));

                double getScore(T t)
                {
                    var actual = getValue(t);
                    return actual == expected ? 1.0 : 0.0;
                }

                return getScore;
            }

            if (targetExpression.Type == typeof(IEnumerable<string>))
            {
                var lambda = Expression.Lambda<Func<T, IEnumerable<string>>>(targetExpression, ctxExpression);
                var getValue = lambda.Compile();
                var expectedString = leafExpression.Right;
                var expectedArray = leafExpression.Right.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim()).ToArray();

                double getScore(T t)
                {
                    var actual = getValue(t)?.ToArray();
                    var commonCount = actual?.Distinct().Intersect(expectedArray).Count() ?? 0;
                    var actualCount = actual?.Count() ?? 0;
                    var expectedCount = expectedArray.Length;
                    switch (leafExpression.Operator)
                    {
                        case Operator.In:
                            return actualCount == 0 ? 0 : (double) commonCount / actualCount;
                        case Operator.ContainsAll:
                            return expectedCount == 0 ? 0 : (double) commonCount / expectedCount;
                        case Operator.NotIn:
                            return actualCount == 0 ? 0 : (double) (actualCount - commonCount) / actualCount;
                        case Operator.Contains:
                            return expectedString != null && actual?.Contains(expectedString) == true ? 1.0 : 0.0;
                        case Operator.NotContains:
                            return expectedString == null || actual?.Contains(expectedString) == false ? 1.0 : 0.0;
                    }

                    return actual == expectedArray ? 1.0 : 0.0;
                }

                return getScore;
            }

            throw new NotSupportedException($"expression {leafExpression.Left} is not supported");
        }

        public static Func<T, double> GetScore<T>(this EvalEvidence evidence)
        {
            if (evidence.LeftType.IsNullableType() && Nullable.GetUnderlyingType(evidence.LeftType).IsNumericType() &&
                (evidence.Expression.Operator == Operator.IsNull || evidence.Expression.Operator == Operator.NotIsNull))
            {
                double? actual = string.IsNullOrEmpty(evidence.Actual?.ToString()) ? default(double?) : double.Parse(evidence.Actual.ToString());
                double getScore(T t)
                {
                    switch (evidence.Expression.Operator)
                    {
                        case Operator.IsNull:
                            return actual.HasValue?  0.0 : 1.0;
                        case Operator.NotIsNull:
                            return actual.HasValue?  1.0 : 0.0;
                        default:
                            throw new NotSupportedException($"operator '{evidence.Expression.Operator}' is not supported for numeric type");
                    }
                }

                return getScore;
            }

            if (evidence.LeftType.IsNumericType())
            {
                double actual = double.Parse(evidence.Actual.ToString());
                double expected =double.Parse(evidence.Expected.ToString());
                double getScore(T t)
                {
                    switch (evidence.Expression.Operator)
                    {
                        case Operator.GreaterThan:
                            return actual > expected ? 1.0 : (actual / expected);
                        case Operator.GreaterOrEqual:
                            return actual >= expected ? 1.0 : actual / expected;
                        case Operator.LessThan:
                            return actual < expected ? 1.0 : (actual - expected) / expected;
                        case Operator.LessOrEqual:
                            return actual <= expected ? 1.0 : (actual - expected) / expected;
                        case Operator.Equals:
                            return Math.Abs(actual - expected) < TOLERANCE ? 1.0 : 0.0;
                        case Operator.NotEquals:
                            return Math.Abs(actual - expected) > TOLERANCE ? 1.0 : 0.0;
                        case Operator.DiffWithinPct:
                            double tolerance = double.Parse(evidence.Expression.OperatorArgs[0]) * expected / 100;
                            return Math.Abs(actual - expected) < tolerance ? 1.0 : 0.0;
                        default:
                            throw new NotSupportedException($"operator '{evidence.Expression.Operator}' is not supported for numeric type");
                    }
                }

                return getScore;
            }

            if (evidence.LeftType.IsGenericType && evidence.LeftType.GetGenericArguments()[0].IsNumericType())
            {
                var actualNumberList = ((JArray) evidence.Actual).Select(t => double.Parse(t.ToString())).ToList();
                var expectedArray = evidence.Expression.Right.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                    .Select(double.Parse).ToArray();
                double getScore(T t)
                {
                    switch (evidence.Expression.Operator)
                    {
                        case Operator.IsEmpty:
                            return actualNumberList.Count == 0 ? 1.0 : 0.0;
                        case Operator.NotIsEmpty:
                            return actualNumberList.Count > 0 ? 1.0 : 0.0;
                        case Operator.AllInRangePct:
                            double pct = double.Parse(evidence.Expression.OperatorArgs[0]) / 100;
                            double min = expectedArray[0] * (1 - pct);
                            double max = expectedArray[1] * (1 + pct);
                            return actualNumberList.All(n => n >= min && n <= max) ? 1.0 : 0.0;
                        default:
                            throw new NotSupportedException($"operator '{evidence.Expression.Operator}' is not supported for numeric list");
                    }
                }

                return getScore;
            }

            if (evidence.LeftType == typeof(string))
            {
                var actual = evidence.Actual.ToString();
                var expectedString = evidence.RightType == typeof(string) ? evidence.Expected.ToString() : null;
                var expectedStringArray = evidence.Expression.Right
                    .Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim()).ToArray();

                double getScore(T t)
                {
                    switch (evidence.Expression.Operator)
                    {
                        case Operator.Equals:
                            if (string.IsNullOrEmpty(expectedString)) return 0.0;
                            return actual.Equals(expectedString, StringComparison.InvariantCultureIgnoreCase) ? 1.0 : 0.0;
                        case Operator.NotEquals:
                            if (string.IsNullOrEmpty(expectedString)) return 0.0;
                            return actual.Equals(expectedString, StringComparison.InvariantCultureIgnoreCase) ? 0.0 : 1.0;
                        case Operator.StartsWith:
                            if (string.IsNullOrEmpty(expectedString)) return 0.0;
                            return actual.StartsWith(expectedString) ? 1.0 : 0.0;
                        case Operator.NotStartsWith:
                            if (string.IsNullOrEmpty(expectedString)) return 0.0;
                            return !actual.StartsWith(expectedString) ? 1.0 : 0.0;
                        case Operator.Contains:
                            if (string.IsNullOrEmpty(expectedString)) return 0.0;
                            return actual.Contains(expectedString) ? 1.0 : 0.0;
                        case Operator.NotContains:
                            if (string.IsNullOrEmpty(expectedString)) return 0.0;
                            return !actual.Contains(expectedString) ? 1.0 : 0.0;
                        case Operator.In:
                            if (expectedStringArray.Length == 0) return 0.0;
                            return expectedStringArray.Contains(actual) ? 1.0 : 0.0;
                        case Operator.NotIn:
                            if (expectedStringArray.Length == 0) return 0.0;
                            return !expectedStringArray.Contains(actual) ? 1.0 : 0.0;
                        default:
                            throw new NotSupportedException($"operator '{evidence.Expression.Operator}' is not supported for string type");
                    }
                }

                return getScore;
            }

            if (evidence.LeftType == typeof(string[]) || evidence.LeftType == typeof(IEnumerable<string>))
            {
                var actualStringList = ((JArray)evidence.Actual).Select(i => i.ToString()).ToList();
                var expectedString = evidence.RightType == typeof(string) ? evidence.Expected.ToString() : null;
                var expectedStringArray = evidence.Expression.Right
                    .Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim()).ToArray();
                var commonCount = actualStringList.Distinct().Intersect(expectedStringArray).Count();
                var actualCount = actualStringList.Count;
                var expectedCount = expectedStringArray.Length;

                double getScore(T t)
                {
                    switch (evidence.Expression.Operator)
                    {
                        case Operator.Contains:
                            if (string.IsNullOrEmpty(expectedString)) return 0.0;
                            return actualStringList.Contains(expectedString) ? 1.0 : 0.0;
                        case Operator.NotContains:
                            if (string.IsNullOrEmpty(expectedString)) return 0.0;
                            return !actualStringList.Contains(expectedString) ? 1.0 : 0.0;
                        case Operator.ContainsAll:
                            if (expectedStringArray.Length == 0) return 0.0;
                            //return expectedStringArray.All(s => actualStringList.Contains(s)) ? 1.0 : 0.0;
                            return commonCount < expectedCount ? (double) commonCount / expectedCount : 1.0;
                        case Operator.NotContainsAll:
                            if (expectedStringArray.Length == 0) return 0.0;
                            //return expectedStringArray.Any(s => !actualStringList.Contains(s)) ? 1.0 : 0.0;
                            return commonCount < expectedCount ? 1.0 : (double) commonCount / expectedCount;
                        case Operator.AllIn:
                            if (expectedStringArray.Length == 0) return 0.0;
                            //return actualStringList.All(s => expectedStringArray.Contains(s)) ? 1.0 : 0.0;
                            return commonCount < actualCount ? (double) commonCount / expectedCount : 1.0;
                        case Operator.NotAllIn:
                            if (expectedStringArray.Length == 0) return 0.0;
                            //return actualStringList.Any(s => !expectedStringArray.Contains(s)) ? 1.0 : 0.0;
                            return commonCount < actualCount ? 1.0 : (double) commonCount / expectedCount;
                        default:
                            throw new NotSupportedException($"operator '{evidence.Expression.Operator}' is not supported for numeric type");
                    }
                }

                return getScore;
            }

            if (evidence.LeftType == typeof(bool))
            {
                bool actual = bool.Parse(evidence.Actual.ToString());
                bool expected = bool.Parse(evidence.Expected.ToString());

                double getScore(T t)
                {
                    return actual == expected ? 1.0 : 0.0;
                }

                return getScore;
            }

            if (Nullable.GetUnderlyingType(evidence.LeftType) != null && (
                evidence.Expression.Operator == Operator.IsNull ||
                evidence.Expression.Operator == Operator.NotIsNull))
            {
                double getScore(T t)
                {
                    switch (evidence.Expression.Operator)
                    {
                        case Operator.IsNull:
                            return evidence.Actual == null ? 1.0 : 0.0;
                        case Operator.NotIsNull:
                            return evidence.Actual == null ? 0.0 : 1.0;
                        default:
                            return 0.0;
                    }
                }

                return getScore;
            }

            if (!evidence.LeftType.IsPrimitiveType())
            {
                var actualObject = evidence.LeftType == typeof(Enumerable)
                    ? null
                    : evidence.Actual;
                var actualList = evidence.LeftType == typeof(Enumerable)
                    ? ((JArray) evidence.Actual).ToList()
                    : null;
                var actualCount = actualList?.Count ?? 0;
                double getScore(T t)
                {
                    switch (evidence.Expression.Operator)
                    {
                        case Operator.IsNull:
                            return actualObject == null ? 1.0 : 0.0;
                        case Operator.NotIsNull:
                            return actualObject == null ? 0.0 : 1.0;
                        case Operator.IsEmpty:
                            return actualCount == 0 ? 1.0 : 0.0;
                        case Operator.NotIsEmpty:
                            return actualCount > 0 ? 1.0 : 0.0;
                        default:
                            throw new NotSupportedException($"operator '{evidence.Expression.Operator}' is not supported for type '{evidence.LeftType.Name}'");
                    }
                }

                return getScore;
            }

            throw new NotSupportedException($"expression with type '{evidence.LeftType.Name}' is not supported");
        }

    }
}