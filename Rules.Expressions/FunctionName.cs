// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FunctionName.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public enum FunctionName
    {
        /// <summary>
        /// Count() on either scalar or complex obj
        /// </summary>
        Count,
        
        /// <summary>
        /// DistinctCount() on either scalar or complex obj
        /// </summary>
        DistinctCount,
        
        /// <summary>
        /// Average() or Average(propPath) on numeric values
        /// </summary>
        Average,
        
        /// <summary>
        /// Max() or Max(propPath) on numeric values
        /// </summary>
        Max,
        
        /// <summary>
        /// Min() or Min(propPath) on numeric values
        /// </summary>
        Min,
        
        /// <summary>
        /// Sum() or Sum(propPath) on numeric values
        /// </summary>
        Sum,
        
        /// <summary>
        /// only for datetime target, arg is positive int followed by one of ['m','h','d']
        /// such as: Ago(10m), Ago(1h), Ago(3d)
        /// </summary>
        Ago,
        
        /// <summary>
        /// Select(propPath)
        /// </summary>
        Select
    }

    public static class FunctionNameExtension
    {
        public static List<string> GetAllFunctionNames()
        {
            return (from object e in Enum.GetValues(typeof(FunctionName)) select e.ToString()).ToList();
        }

        public static List<string> GetFunctionNameRegexPatterns()
        {
            var functionNames = GetAllFunctionNames();
            return functionNames.Select(f => $@"^({f})\(([^\(\)]*)\)$").ToList();
        }
    }
}