// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MockPropExpressionBuilder.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions.Tests.TestModels.IoT
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Builders;
    using Helpers;

    public class MockPropExpressionBuilder : IPropertyExpression
    {
        public List<MethodInfo> GetMacroMethods(Type owner)
        {
            throw new NotImplementedException();
        }

        public bool CanQuery(Type owner, PropertyInfo prop)
        {
            throw new NotImplementedException();
        }

        public bool CanSelect(Type owner, PropertyInfo prop)
        {
            throw new NotImplementedException();
        }

        public bool CanCompare(Type owner, PropertyInfo prop)
        {
            throw new NotImplementedException();
        }

        public bool CanSort(Type owner, PropertyInfo prop)
        {
            throw new NotImplementedException();
        }
    }
}