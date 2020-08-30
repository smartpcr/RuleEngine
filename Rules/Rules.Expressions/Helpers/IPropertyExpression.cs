// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPropertyExpression.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public interface IPropertyExpression
    {
        List<MethodInfo> GetMacroMethods(Type owner);
        bool CanQuery(Type owner, PropertyInfo prop);
        bool CanSelect(Type owner, PropertyInfo prop);

        bool CanCompare(Type owner, PropertyInfo prop);
        bool CanSort(Type owner, PropertyInfo prop);
    }
}