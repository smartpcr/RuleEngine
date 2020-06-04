// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Last.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions.FunctionExpression
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;

    public class LastExpression: FunctionExpression
    {
        private readonly string fieldName;
        private readonly Operator op;
        private readonly string fieldValue;
        private readonly Type argType;
        
        public LastExpression(Expression target, FunctionName funcName, params string[] args) : base(target, funcName, args)
        {
            if (args != null && args.Length != 3 && args.Length != 0)
            {
                throw new ArgumentException($"exactly 0 or 3 args required for function '{funcName}'");
            }

            if (args?.Length == 3)
            {
                fieldName = args[0];
                op = (Operator) Enum.Parse(typeof(Operator), args[1], true);
                fieldValue = args[2];
            }
            
            if (Target.Type.IsGenericType)
            {
                argType = Target.Type.GetGenericArguments()[0];    
            }
            else if (Target.Type.IsArray)
            {
                argType = Target.Type.GetElementType();
            }
            else
            {
                throw new InvalidOperationException($"target expression type '{Target.Type.Name}' is invalid, it should be either an array or enumerable");
            }
        }

        public override Expression Build()
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                return Expression.Call(
                    typeof(Enumerable),
                    "First",
                    new[] {argType},
                    Target);
            }
            
            var argParameter = Expression.Parameter(argType, "s");
            var propNames = fieldName.Split(new[] {'.'});
            Expression propExpression = argParameter;
            foreach (var propName in propNames)
            {
                var prop = propExpression.Type.GetMappedProperty(propName);
                propExpression = Expression.Property(propExpression, prop);
            }
            Expression valueExpr = Expression.Constant(fieldValue);
            if (valueExpr.Type != propExpression.Type)
            {
                valueExpr = Expression.Convert(valueExpr, propExpression.Type);
            }
            
            Expression predicate;
            switch (op)
            {
                case Operator.Equals:
                    predicate = Expression.Equal(propExpression, valueExpr);
                    break;
                case Operator.NotEquals:
                    predicate = Expression.NotEqual(propExpression, valueExpr);
                    break;
                case Operator.GreaterThan:
                    predicate = Expression.GreaterThan(propExpression, valueExpr);
                    break;
                case Operator.GreaterOrEqual:
                    predicate = Expression.GreaterThanOrEqual(propExpression, valueExpr);
                    break;
                case Operator.LessThan:
                    predicate = Expression.LessThan(propExpression, valueExpr);
                    break;
                case Operator.LessOrEqual:
                    predicate = Expression.LessThanOrEqual(propExpression, valueExpr);
                    break;
                default:
                    throw new NotSupportedException($"operator {op} is not supported in function '{FuncName}'");
            }
            
            var predicateExpr = Expression.Lambda(predicate, argParameter);
            return Expression.Call(
                typeof(Enumerable),
                "Last",
                new[] {argType},
                Target,
                predicateExpr);
        }
    }
}