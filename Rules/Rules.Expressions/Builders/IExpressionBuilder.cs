// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IExpressionBuilder.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions.Builders
{
    using System.Collections.Generic;

    public interface IExpressionBuilder<T> where T : class
    {
        List<PropertyPath> Next(string current);
        List<string> GetApplicableOperators(string propPath);
    }
}