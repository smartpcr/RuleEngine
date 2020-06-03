// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AgoExpression.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions.FunctionExpression
{
    using System;
    using System.Linq.Expressions;
    using System.Text.RegularExpressions;

    public class AgoExpression : FunctionExpression
    {
        private readonly TimeSpan span;
        private static readonly Regex argRegex = new Regex(@"(\d+)(m|h|d)", RegexOptions.Compiled);
        
        public AgoExpression(Expression target, FunctionName funcName, params string[] args) : base(target, funcName, args)
        {
            if (args == null || args.Length != 1)
            {
                throw new ArgumentException($"Exactly one argument is required for function '{funcName}'");
            }
            var funcArg = args[0];
            var match = argRegex.Match(funcArg);
            if (!match.Success)
            {
                throw new InvalidOperationException($"invalid arg '{funcArg}' for function {funcName}");
            }

            var number = int.Parse(match.Groups[1].Value);
            switch (match.Groups[2].Value)
            {
                case "m":
                    span = TimeSpan.FromMinutes(0 - number);
                    break;
                case "h":
                    span = TimeSpan.FromHours(0 - number);
                    break;
                case "d":
                    span = TimeSpan.FromDays(0 - number);
                    break;
                default:
                    throw new InvalidOperationException($"invalid arg '{funcArg}' for function {funcName}");
            }
        }

        public override MethodCallExpression Create()
        {
            var now = Expression.Constant(DateTime.UtcNow);
            var spanExpr = Expression.Constant(span);
            var method = typeof(DateTime).GetMethod("Add");
            if (method == null)
            {
                throw new InvalidOperationException("method 'Add' not found on DateTime type");
            }
            
            return Expression.Call(now, method, spanExpr);
        }
    }
}