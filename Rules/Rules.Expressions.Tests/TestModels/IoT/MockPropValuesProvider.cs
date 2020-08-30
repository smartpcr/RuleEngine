// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MockPropValuesProvider.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions.Tests.TestModels.IoT
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading.Tasks;
    using Helpers;

    public class MockPropValuesProvider : IPropertyValuesProvider
    {
        public Task<IEnumerable<string>> GetAllowedValues(Type owner, PropertyInfo prop)
        {
            throw new NotImplementedException();
        }
    }
}