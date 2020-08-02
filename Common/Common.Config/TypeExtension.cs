// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TypeExtension.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Common.Config
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    public static class TypeExtension
    {
        public static IEnumerable<MethodInfo> GetExtensionMethods(this Type extendedType)
        {
            var query = from type in extendedType.Assembly.GetTypes()
                where type.IsSealed && !type.IsGenericType && !type.IsNested
                from method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                where method.IsDefined(typeof(ExtensionAttribute), false)
                where method.GetParameters()[0].ParameterType == extendedType
                select method;
            return query;
        }
    }
}