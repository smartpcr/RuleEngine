// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MethodInspect.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions
{
    using System;

    public class MethodInspect
    {
        public MethodInspect(string methodName, Type targetType, Type argumentType, Type extensionType)
        {
            MethodName = methodName;
            TargetType = targetType;
            ArgumentType = argumentType;
            ExtensionType = extensionType;
        }

        public string MethodName { get; }
        public Type TargetType { get; }
        public Type ArgumentType { get; }
        public Type ExtensionType { get; }
    }
}