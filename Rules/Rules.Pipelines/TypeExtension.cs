// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TypeExtension.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Engines
{
    using System;
    using System.Collections.Generic;

    public static class TypeExtension
    {
        public static bool IsListOf(this Type type, Type itemType)
        {
            return type.IsGenericType &&
                   type.GetGenericTypeDefinition() == typeof(IList<>) &&
                   type.GetGenericArguments()[0] == itemType;
        }

        public static bool IsArrayOf(this Type type, Type itemType)
        {
            return type.IsArray && type.GetElementType() == itemType;
        }
    }
}