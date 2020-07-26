// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Assert.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Script
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Scripting;
    using Microsoft.CodeAnalysis.Scripting;

    public class Assert<T> where T: class, new()
    {
        private readonly string expression;
        private readonly ScriptOptions options;

        public Assert(string expression)
        {
            this.expression = expression;
            options = ScriptOptions.Default.AddReferences(typeof(T).Assembly);
        }

        public async Task<Func<T, bool>> Compile()
        {
            var assert = await CSharpScript.EvaluateAsync<Func<T, bool>>(expression, options);
            return assert;
        }
    }
}