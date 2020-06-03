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
        Select,
        
        /// <summary>
        /// Where(fieldName, operator, fieldValue)
        /// fieldName is prop name
        /// operator can only be binary: equals, notEquals, greaterThan, greaterOrEqual, lessThan, lessOrEqual
        /// fieldValue must be constant
        /// </summary>
        Where,
        
        /// <summary>
        /// Traverse(propName, idPropName, steps)
        /// propName: must return instance of the same type
        /// idPropName: unique prop of type string, helps tracking loops
        /// steps: number of steps taken until stop, default to -1 and continue until next traverse returns null
        /// returns IEnumerable of instances (empty if it's already at root)
        /// returns null when circular-reference found
        /// </summary>
        Traverse
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