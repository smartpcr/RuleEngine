// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EvidenceExtension.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions.Eval
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Helpers;
    using Newtonsoft.Json.Linq;

    public static class EvidenceExtension
    {
        public static Func<T, string> GetEvidence<T>(this LeafExpression leafExpression)
        {
            return BuildGetStringFunc<T>(
                leafExpression.Left,
                leafExpression.Operator != Operator.IsNull && leafExpression.Operator != Operator.NotIsNull);
        }

        public static Func<T, string> GetExpectation<T>(this LeafExpression leafExpression)
        {
            Func<T, string> getExpectation = leafExpression.RightSideIsExpression
                ? BuildGetStringFunc<T>(leafExpression.Right, true, leafExpression.Operator.ToString())
                : t => $"{leafExpression.Operator.ToString()} {leafExpression.Right}";
            return getExpectation;
        }

        public static List<EvalEvidence> GetEvidence<T>(this IConditionExpression condition, T instance)
        {
            var leafEvaluators = new List<LeafExpression>();
            PopulateLeafFieldEvaluators(condition, leafEvaluators);
            var list = new List<EvalEvidence>();
            foreach (var leafExpr in leafEvaluators)
            {
                var ctxParameter = Expression.Parameter(typeof(T), "ctx");
                var leftExpression =
                    ctxParameter.BuildExpression(leafExpr.Left, leafExpr.Operator != Operator.IsNull && leafExpr.Operator != Operator.NotIsNull);
                var leftType = leftExpression.Type;
                var lambda = Expression.Lambda(leftExpression, ctxParameter);
                var getValue = lambda.Compile();
                var actualObj = getValue.DynamicInvoke(instance);

                object expected = leafExpr.Right;
                Type rightType = typeof(string);
                if (leafExpr.RightSideIsExpression)
                {
                    var rightExpression = ctxParameter.BuildExpression(leafExpr.Right);
                    lambda = Expression.Lambda(rightExpression, ctxParameter);
                    getValue = lambda.Compile();
                    var expectedObj = getValue.DynamicInvoke(instance);
                    expected = expectedObj;
                    rightType = rightExpression.Type;
                }

                var evidence = new EvalEvidence()
                {
                    Expression = leafExpr,
                    LeftType = leftType,
                    RightType = rightType,
                    Actual = actualObj == null ? null : JToken.FromObject(actualObj),
                    Expected = JToken.FromObject(expected)
                };
                var getScore = evidence.GetScore<T>();
                evidence.Score = getScore(instance);
                list.Add(evidence);
            }

            return list;
        }

        public static void PopulateLeafFieldEvaluators(this IConditionExpression condition,
            List<LeafExpression> leafEvaluators)
        {
            if (condition is LeafExpression leaf)
                leafEvaluators.Add(leaf);
            else if (condition is AllOfExpression allOf)
                foreach (var leafExpr in allOf.AllOf)
                    PopulateLeafFieldEvaluators(leafExpr, leafEvaluators);
            else if (condition is AnyOfExpression anyOf)
                foreach (var leafExpr in anyOf.AnyOf)
                    PopulateLeafFieldEvaluators(leafExpr, leafEvaluators);
        }

        private static Func<T, string> BuildGetStringFunc<T>(string leafExpressionCondition, bool handleNullableType = true, string prefix = null)
        {
            var ctxExpression = Expression.Parameter(typeof(T), "ctx");
            var targetExpression = ctxExpression.BuildExpression(leafExpressionCondition, handleNullableType);
            Func<T, string> toString = null;
            if (targetExpression.Type == typeof(int))
            {
                var lambda = Expression.Lambda<Func<T, int>>(targetExpression, ctxExpression);
                var getValue = lambda.Compile();

                toString = t => $"{prefix ?? ""} {getValue(t).ToString()}";
            }

            if (targetExpression.Type == typeof(double))
            {
                var lambda = Expression.Lambda<Func<T, double>>(targetExpression, ctxExpression);
                var getValue = lambda.Compile();

                toString = t => (prefix ?? "") + getValue(t).ToString("##.###");
            }

            if (targetExpression.Type == typeof(decimal))
            {
                var lambda = Expression.Lambda<Func<T, decimal>>(targetExpression, ctxExpression);
                var getValue = lambda.Compile();

                toString = t =>  (prefix ?? "") + getValue(t).ToString("##.###");
            }

            if (targetExpression.Type == typeof(string))
            {
                var lambda = Expression.Lambda<Func<T, string>>(targetExpression, ctxExpression);
                var getValue = lambda.Compile();
                toString = t => $"{prefix ?? ""} {getValue(t).ToString()}";
            }

            if (targetExpression.Type == typeof(bool))
            {
                var lambda = Expression.Lambda<Func<T, bool>>(targetExpression, ctxExpression);
                var getValue = lambda.Compile();
                toString = t => $"{prefix ?? ""} {getValue(t).ToString()}";
            }

            if (targetExpression.Type == typeof(IEnumerable<string>))
            {
                var lambda = Expression.Lambda<Func<T, IEnumerable<string>>>(targetExpression, ctxExpression);
                var getValue = lambda.Compile();

                toString = t => (prefix ?? "") + string.Join(",", getValue(t));
            }

            if (toString == null)
            {
                throw new NotSupportedException($"expression {leafExpressionCondition} is not supported");
            }

            return toString;
        }
    }
}