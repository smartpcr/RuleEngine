// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExpressionBuilder.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions.Evaluators
{
    using System;
    using System.Linq.Expressions;
    using Rules.Expressions;

    public interface IExpressionBuilder
    {
        Func<T, bool> Build<T>(IConditionExpression conditionExpression) where T : class;

        Delegate Build(IConditionExpression conditionExpression, Type contextType);

    }

    public class ExpressionBuilder : IExpressionBuilder
    {
        public Func<T, bool> Build<T>(IConditionExpression conditionExpression) where T : class
        {
            var contextType = typeof(T);
            var contextParameter = Expression.Parameter(contextType, "ctx");
            var expression = conditionExpression.Process(contextParameter, contextType);
            var @delegate = Expression.Lambda<Func<T, bool>>(expression, contextParameter);
            var func = @delegate.Compile();
            return func;
        }

        [Obsolete("should use alternative method that pass in generic type, since this method requires call to DynamicInvoke, which is slow")]
        public Delegate Build(IConditionExpression conditionExpression, Type contextType)
        {
            var contextParameter = Expression.Parameter(contextType, "ctx");
            var expression = conditionExpression.Process(contextParameter, contextType);
            var lambda = Expression.Lambda(expression, contextParameter);
            var func = lambda.Compile();
            return func;
        }
    }
}