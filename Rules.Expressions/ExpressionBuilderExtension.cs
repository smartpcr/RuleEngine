// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExpressionBuilderExtension.cs" company="Microsoft Corporation">
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
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;
    using FunctionExpression;
    using Rules.Expressions.Macros;

    public static class ExpressionBuilderExtension
    {
        public static Expression BuildExpression(this ParameterExpression contextExpression, string propPath)
        {
            var parts = SplitPropPath(propPath);
            Expression targetExpression = contextExpression;
            foreach (var part in parts)
            {
                if (targetExpression.TryFindMacro(part, out var macroExpr))
                    targetExpression = macroExpr;
                else if (targetExpression.TryFindFunction(part, out var funcExpr))
                    targetExpression = funcExpr;
                else if (targetExpression.TryFindIndexerField(part, out var arrayItemExpr))
                    targetExpression = arrayItemExpr;
                else if (targetExpression.TryFindProperty(part, out var propExpression))
                    targetExpression = propExpression;
                else
                    throw new InvalidOperationException(
                        $"failed to evaluate part {part} on type {targetExpression.Type.Name}");

                if (HandleEnumExpression(targetExpression, out var enumExpression)) targetExpression = enumExpression;

                if (HandleNullableType(targetExpression, out var valueExpression)) targetExpression = valueExpression;
            }

            return targetExpression;
        }

        public static Expression AddToStringWithEnumType(this Expression targetExpression)
        {
            var addToStringMethod = false;
            var targetType = targetExpression.Type;
            if (targetType.IsEnum)
            {
                addToStringMethod = true;
            }
            else
            {
                var underlyingType = Nullable.GetUnderlyingType(targetType);
                if (underlyingType?.IsEnum == true)
                {
                    addToStringMethod = true;
                }
            }

            if (addToStringMethod)
            {
                var toStringMethod = targetType.GetMethod("ToString", Type.EmptyTypes);
                if (toStringMethod == null) throw new Exception($"type {targetType.Name} doesn't have ToString method");
                var toStringCall = Expression.Call(targetExpression, toStringMethod);
                return toStringCall;
            }

            return targetExpression;
        }

        public static Expression AddValueWithNullableNumberType(this Expression targetExpression)
        {
            var targetType = targetExpression.Type;
            var underlyingType = Nullable.GetUnderlyingType(targetType);
            if (underlyingType != null)
            {
                var toValueCall = Expression.Property(targetExpression, "Value");
                return toValueCall;
            }

            return targetExpression;
        }

        public static bool AddNotNullCheck(this Expression targetExpression, out Expression notNullCheckExpression)
        {
            notNullCheckExpression = null;
            var targetType = targetExpression.Type;
            if (!targetType.IsPrimitiveType())
            {
                var nullExpr = Expression.Constant(null, typeof(object));
                notNullCheckExpression = Expression.Not(Expression.Equal(targetExpression, nullExpr));
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// using "." as delimiter but skip anything inside parenthesis
        /// note: there can be nested function inside another function
        /// </summary>
        /// <param name="propPath"></param>
        /// <returns></returns>
        public static string[] SplitPropPath(this string propPath)
        {
            var parts = new List<string>();
            var left = propPath.IndexOf("(", 0, StringComparison.Ordinal);
            if (left < 0)
            {
                return propPath.Split(new[] {'.'}, StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim()).ToArray();
            }
            
            var stack = new Stack<int>();
            var queue = new Queue<(int left, int right)>();
            for (var i = 0; i < propPath.Length; i++)
            {
                var c = propPath[i];
                if (c == '(')
                {
                    stack.Push(i);
                }
                else if (c == ')')
                {
                    left = stack.Pop();
                    if (stack.Count == 0)
                    {
                        queue.Enqueue((left, i));
                    }
                }
            }

            if (stack.Count > 0)
            {
                throw new InvalidOperationException($"imbalanced expression found: '(' at {stack.Pop()} is not matched");
            }

            if (queue.Count <= 0)
            {
                throw new InvalidOperationException($"unable to capture any enclosed parenthesis");
            }

            var prevPos = 0;
            while (queue.Count > 0)
            {
                var grouped = queue.Dequeue();
                if (grouped.left > prevPos)
                {
                    var pathBeforeGroup = propPath.Substring(prevPos, grouped.left - prevPos);
                    var partsBeforeGroup = pathBeforeGroup.Split(new[] {'.'}, StringSplitOptions.RemoveEmptyEntries);
                    for (var i = 0; i < partsBeforeGroup.Length - 1; i++)
                    {
                        parts.Add(partsBeforeGroup[i]);
                    }

                    var funcName = partsBeforeGroup[partsBeforeGroup.Length - 1];
                    var funcArgs = propPath.Substring(grouped.left, grouped.right + 1 - grouped.left);
                    var funcDef = $"{funcName}{funcArgs}";
                    parts.Add(funcDef);
                    prevPos = grouped.right + 1;
                }
                else
                {
                    throw new InvalidOperationException($"missing function name at {grouped.left}");
                }
            }

            if (prevPos < propPath.Length)
            {
                var pathAfterGroup = propPath.Substring(prevPos);
                parts.AddRange(pathAfterGroup.Split(new []{'.'}, StringSplitOptions.RemoveEmptyEntries));
            }
            
            return parts.ToArray();
        }
        
        private static bool TryFindMacro(
            this Expression parentExpression,
            string field,
            out Expression macroExpression)
        {
            macroExpression = null;
            
            var methodRegex = new Regex(@"^(\w+)\(([^\(\)]*)\)$");
            if (methodRegex.IsMatch(field))
            {
                var macroName = methodRegex.Match(field).Groups[1].Value;
                var macroArgs = methodRegex.Match(field).Groups[2].Value;
                var args = macroArgs.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).Select(a => a.Trim()).ToArray();
                var extensionMethods = GetExtensionMethods(parentExpression.Type);
                if (extensionMethods?.Any() == true)
                {
                    var extensionMethod = extensionMethods.FirstOrDefault(m => m.Name == macroName);
                    if (extensionMethod != null)
                    {
                        var methodParameters = extensionMethod.GetParameters();
                        if (methodParameters.Length == args.Length + 1 && methodParameters[0].ParameterType == parentExpression.Type)
                        {
                            var macroCreator = new MacroExpressionCreator(parentExpression, extensionMethod, args);
                            macroExpression = macroCreator.CreateMacroExpression();
                            return true;    
                        }
                    }
                }
            }
            
            return false;
        }
        
        private static bool TryFindFunction(
            this Expression parentExpression,
            string field,
            out Expression functionExpression)
        {
            functionExpression = null;
            var functionRegexPatterns = FunctionNameExtension.GetFunctionNameRegexPatterns();
            foreach (var pattern in functionRegexPatterns)
            {
                var regex = new Regex(pattern);
                var match = regex.Match(field);
                if (match.Success)
                {
                    var functionName = match.Groups[1].Value;
                    var functionArg = match.Groups[2].Value;
                    var args = functionArg.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).Select(a => a.Trim()).ToArray();
                    var funcName = (FunctionName)Enum.Parse(typeof(FunctionName), functionName);
                    var funcExpr = new FunctionExpressionCreator().Create(parentExpression, funcName, args);
                    functionExpression = funcExpr.Build();
                    return true;
                }
            }
            
            return false;
        }
        
        private static bool TryFindIndexerField(
            this Expression parentExpression,
            string field,
            out Expression arrayExpression)
        {
            // Indexer access field should be of the form property['<key>'] or property[<key>]
            arrayExpression = null;
            var leftBracketIndex = field.IndexOf("[", StringComparison.Ordinal);
            if (leftBracketIndex > 0)
            {
                var rightBracketIndex = field.IndexOf("]", leftBracketIndex, StringComparison.Ordinal);
                if (rightBracketIndex != -1)
                {
                    var key = field.Substring(leftBracketIndex + 1, rightBracketIndex - leftBracketIndex - 1);
                    key = key.Trim('\'');
                    var propertyName = field.Substring(0, leftBracketIndex);
                    if (!parentExpression.TryFindProperty(propertyName, out var propExpression) ||
                        propExpression == null)
                        throw new InvalidOperationException(
                            $"failed to get array property {propertyName} on type {parentExpression.Type.Name}");
                    if (int.TryParse(key, out var index))
                    {
                        var collectionType = propExpression.Type;
                        if (collectionType.IsGenericType)
                        {
                            propExpression = Expression.Call(
                                typeof(Enumerable),
                                "ToArray",
                                new[] {collectionType.GetGenericArguments()[0]},
                                propExpression);
                            arrayExpression = Expression.ArrayIndex(propExpression, Expression.Constant(index));
                        }
                        else
                        {
                            arrayExpression = Expression.ArrayIndex(propExpression, Expression.Constant(index));
                        }
                    }
                    else
                    {
                        var keyExpression = Expression.Constant(key);
                        arrayExpression = Expression.Property(propExpression, "Item", keyExpression);
                    }

                    return true;
                }
            }

            return false;
        }
        
        private static bool TryFindProperty(
            this Expression parentExpression,
            string field,
            out Expression propExpression)
        {
            propExpression = null;
            var prop = parentExpression.Type.GetMappedProperty(field);
            if (prop != null)
            {
                propExpression = Expression.Property(parentExpression, prop);
                return true;
            }

            return false;
        }
        
        private static bool HandleEnumExpression(
            this Expression parentExpression,
            out Expression enumExpression)
        {
            enumExpression = null;
            var fieldType = parentExpression.Type;
            var underlyingType = Nullable.GetUnderlyingType(fieldType);
            if (fieldType.IsEnum || underlyingType?.IsEnum == true)
            {
                var toStringMethod = fieldType.GetMethod("ToString", Type.EmptyTypes);
                if (toStringMethod == null) throw new Exception($"type {fieldType.Name} doesn't have ToString method");
                enumExpression = Expression.Call(parentExpression, toStringMethod);
                return true;
            }

            return false;
        }

        private static bool HandleNullableType(
            this Expression parentExpression,
            out Expression valueExpression)
        {
            valueExpression = null;
            var fieldType = parentExpression.Type;
            var underlyingType = Nullable.GetUnderlyingType(fieldType);
            if (underlyingType?.IsNumericType() == true)
            {
                valueExpression = Expression.Property(parentExpression, "Value");
                return true;
            }

            return false;
        }

        private static IEnumerable<MethodInfo> GetExtensionMethods(Type extendedType)
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