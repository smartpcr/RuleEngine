// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPropertyExpression.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Common.Config
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading.Tasks;

    public interface IPropertyExpression
    {
        List<Type> SupportedTypes { get; }
        List<MethodInfo> GetMacroMethods(Type owner);
        bool CanQuery(Type owner, PropertyInfo prop);
        bool CanSelect(Type owner, PropertyInfo prop);

        bool CanCompare(Type owner, PropertyInfo prop);
        bool CanSort(Type owner, PropertyInfo prop);
    }

    public interface IPropertyValuesProvider
    {
        Task<IEnumerable<string>> GetAllowedValues(Type owner, PropertyInfo prop);
    }
}