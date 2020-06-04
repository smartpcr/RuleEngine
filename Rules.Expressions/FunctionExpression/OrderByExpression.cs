// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OrderByExpression.cs" company="Microsoft Corporation">
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

    public class OrderByExpression : FunctionExpression
    {
        private readonly string orderByField;
        
        public OrderByExpression(Expression target, FunctionName funcName, params string[] args) : base(target, funcName, args)
        {
            if (args == null || args.Length != 1)
            {
                throw new ArgumentException($"exactly one argument expected for function '{funcName}'");
            }

            orderByField = args[0];
        }

        public override Expression Build()
        {
            Type itemType = null;
            if (Target.Type.IsGenericType)
            {
                itemType = Target.Type.GetGenericArguments()[0];    
            }
            else if (Target.Type.IsArray)
            {
                itemType = Target.Type.GetElementType();
            }
            
            if (itemType == null)
            {
                throw new InvalidOperationException($"target type '{Target.Type.Name}' of select function is not supported");
            }
            
            var prop = itemType.GetMappedProperty(orderByField);
            var argParameter = Expression.Parameter(itemType, "_");
            var propExpression = Expression.Property(argParameter, prop);
            Expression selectorExpression = Expression.Lambda(propExpression, argParameter);
            
            return Expression.Call(
                typeof(Enumerable),
                "OrderBy",
                new []{itemType, propExpression.Type},
                Target,
                selectorExpression);
        }
    }
}