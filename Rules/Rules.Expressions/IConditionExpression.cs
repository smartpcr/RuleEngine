// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IConditionExpression.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions
{
    using System;
    using System.Linq.Expressions;

    public interface IConditionExpression
    {
        Expression Process(ParameterExpression parameterExpression, Type parameterType);
    }
}