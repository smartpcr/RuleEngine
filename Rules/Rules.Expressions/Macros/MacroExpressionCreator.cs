﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MacroExpressionCreator.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions.Macros
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;

    public class MacroExpressionCreator
    {
        private readonly Expression parentExpression;
        private readonly MethodInfo macroMethod;
        private readonly string[] args;

        public MacroExpressionCreator(Expression parentExpression, MethodInfo macroMethod, string[] args)
        {
            this.parentExpression = parentExpression;
            this.macroMethod = macroMethod;
            this.args = args;
        }

        public Expression CreateMacroExpression()
        {
            var inputParameters = macroMethod.GetParameters();
            if (inputParameters.Length == 1)
            {
                return Expression.Call(null, macroMethod, parentExpression);
            }

            var argExpressions = new List<Expression>();
            argExpressions.Add(parentExpression);
            for (var i = 1; i < inputParameters.Length; i++)
            {
                object arg = args[i - 1];
                var parameter = inputParameters[i];
                if (arg.GetType() != parameter.ParameterType)
                {
                    arg = Convert.ChangeType(arg, parameter.ParameterType);
                }
                var paramExpr = Expression.Convert(Expression.Constant(arg), parameter.ParameterType);
                argExpressions.Add(paramExpr);
            }
            return Expression.Call(null, macroMethod, argExpressions.ToArray());
        }
    }
}