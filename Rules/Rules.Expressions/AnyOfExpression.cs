// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AnyOfExpression.cs" company="Microsoft Corporation">
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

    public class AnyOfExpression : IConditionExpression
    {
        [JsonProperty(Required = Required.Always)]
        public IConditionExpression[] AnyOf { get; set; }

        public Expression Process(ParameterExpression parameterExpression, Type parameterType)
        {
            if (AnyOf.Length == 0) throw new FormatException(Resources.MissingChildConditionFormatException);
            if (AnyOf.Length == 1) return AnyOf[0].Process(parameterExpression, parameterType);
            var expression = Expression.OrElse(AnyOf[0].Process(parameterExpression, parameterType),
                AnyOf[1].Process(parameterExpression, parameterType));
            for (var i = 2; i < AnyOf.Length; i++)
                expression = Expression.OrElse(expression, AnyOf[i].Process(parameterExpression, parameterType));

            return expression;
        }
    }
}