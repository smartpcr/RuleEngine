namespace Rules.Expressions.Evaluators
{
    using System;

    public interface IExpressionEvaluator
    {
        Func<T, bool> Evaluate<T>(IConditionExpression conditionExpression) where T : class;

        Delegate Evaluate(IConditionExpression conditionExpression, Type contextType);

    }
}