// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompoundExpression.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions
{
    using System;
    using System.Linq.Expressions;
    using Newtonsoft.Json;

    public class AllOfExpression : IConditionExpression
    {
        [JsonProperty(Required = Required.Always)]
        public IConditionExpression[] AllOf { get; set; }
        
        public Expression Process(ParameterExpression parameterExpression, Type parameterType)
        {
            if (AllOf.Length == 0) throw new FormatException("Aggregated operators must have at least one child condition");
            if (AllOf.Length == 1) return AllOf[0].Process(parameterExpression, parameterType);
            
            var expression = Expression.AndAlso(AllOf[0].Process(parameterExpression, parameterType),
                AllOf[1].Process(parameterExpression, parameterType));
            for (var i = 2; i < AllOf.Length; i++)
            {
                expression = Expression.AndAlso(expression, AllOf[i].Process(parameterExpression, parameterType));
            }
            return expression;
        }
    }
}