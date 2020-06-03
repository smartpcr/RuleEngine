// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NotExpression.cs" company="Microsoft Corporation">
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

    public class NotExpression : IConditionExpression
    {
        [JsonProperty(Required = Required.Always)]
        public IConditionExpression Not { get; set; }

        public Expression Process(ParameterExpression parameterExpression, Type parameterType)
        {
            return Expression.Not(Not.Process(parameterExpression, parameterType));
        }
    }
}