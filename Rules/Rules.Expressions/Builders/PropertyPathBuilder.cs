// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PropertyPathExtension.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions.Builders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Common.Config;
    using Evaluators;
    using Helpers;

    public class PropertyPathBuilder<T>: IExpressionBuilder<T> where T: class
    {
        private readonly IPropertyValuesProvider propValuesProvider;
        private readonly List<FunctionName> aggregateFunctions;
        private readonly List<FunctionName> memberAggregateFunctions;
        
        public PropertyPathBuilder(IPropertyValuesProvider propValuesProvider)
        {
            this.propValuesProvider = propValuesProvider;
            aggregateFunctions = Enum.GetValues(typeof(FunctionName)).OfType<FunctionName>().Where(f => f.IsAggregateFunction()).ToList();
            memberAggregateFunctions = Enum.GetValues(typeof(FunctionName)).OfType<FunctionName>().Where(f => f.AllowMemberAggregate()).ToList();
        }

        public List<PropertyPath> Next(string current = "")
        {
            var contextType = typeof(T);
            var contextParameter = Expression.Parameter(contextType, "ctx");
            Expression targetExpression = contextParameter;
            if (!string.IsNullOrEmpty(current))
            {
                targetExpression = contextParameter.EvaluateExpression(current);
            }

            var nextParts = new List<PropertyPath>();
            var currentType = targetExpression.Type;
            if (currentType.IsScalarType())
            {
                // break
            }
            else if (currentType.IsArray || currentType.IsGenericType)
            {
                var elementType = currentType.IsArray
                    ? currentType.GetElementType()
                    : currentType.GetGenericArguments()[0];
                // aggregates
                foreach (var aggregateFunc in aggregateFunctions)
                {
                    nextParts.Add(new PropertyPath($"{aggregateFunc}()", aggregateFunc.ReturnTypeIsInt() ? typeof(int) : elementType));
                }

                if (elementType.IsScalarType())
                {
                }
                else
                {
                    // select, where, orderBy, orderByDesc methods
                    var props = elementType!.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(
                        p => p.CanRead && p.PropertyType.IsScalarType())
                        .ToList();
                    foreach (var prop in props)
                    {
                        nextParts.Add(new PropertyPath($"Select({prop.Name})", typeof(IEnumerable<>).MakeGenericType(prop.PropertyType)));
                    }

                    var grandChildrenProps = elementType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                        .Where(p => p.PropertyType.IsArray || p.PropertyType.IsGenericType).ToList();
                    foreach (var prop in grandChildrenProps)
                    {
                        var childElementType = prop.PropertyType.IsArray
                            ? prop.PropertyType.GetElementType()
                            : prop.PropertyType.GetGenericArguments()[0];
                        nextParts.Add(new PropertyPath($"SelectMany({prop.Name})", typeof(IEnumerable<>).MakeGenericType(childElementType!)));
                    }
                    
                    nextParts.Add(new PropertyPath($"Where()", currentType){ArgumentCount = 3});
                    
                    props = elementType.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(
                            p => p.CanRead && (
                                p.PropertyType.IsNumericType() || Nullable.GetUnderlyingType(p.PropertyType)?.IsNumericType() == true || 
                                p.PropertyType == typeof(DateTime) || Nullable.GetUnderlyingType(p.PropertyType) == typeof(DateTime)))
                        .ToList();
                    foreach (var prop in props)
                    {
                        nextParts.Add(new PropertyPath($"OrderBy({prop.Name})", typeof(IEnumerable<>).MakeGenericType(elementType)));
                        nextParts.Add(new PropertyPath($"OrderByDesc({prop.Name})", typeof(IEnumerable<>).MakeGenericType(elementType)));
                        
                        foreach (var memberAggregateFunc in memberAggregateFunctions)
                        {
                            nextParts.Add(new PropertyPath($"{memberAggregateFunc}({prop.Name})", prop.PropertyType));
                        }
                    }
                }
            }
            else
            {
                var props = currentType.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.CanRead).ToList();
                foreach (var prop in props)
                {
                    nextParts.Add(new PropertyPath(prop.Name, prop.PropertyType) {AllowedValues = GetAllowedValues(currentType, prop)});
                }
                
                var extensionMethods = currentType.GetExtensionMethods().Where(m => m.GetParameters().Length > 1).ToList();
                foreach (var extMethod in extensionMethods)
                {
                    nextParts.Add(new PropertyPath($"{extMethod.Name}()", extMethod.ReturnType){ArgumentCount = extMethod.GetParameters().Length - 1});
                }
            }

            return nextParts;
        }

        #region operators

        public List<string> GetApplicableOperators(string propPath)
        {
            var contextType = typeof(T);
            var contextParameter = Expression.Parameter(contextType, "ctx");
            Expression targetExpression = contextParameter;
            if (!string.IsNullOrEmpty(propPath))
            {
                targetExpression = contextParameter.EvaluateExpression(propPath);
            }
            var currentType = targetExpression.Type;
            return GetOperatorsForType(currentType);
        }

        private List<string> GetOperatorsForType(Type type)
        {
            if (type.IsPrimitiveType())
            {
                if (type.IsEnum || Nullable.GetUnderlyingType(type)?.IsEnum == true)
                {
                    return new List<Operator>()
                    {
                        Operator.Equals,
                        Operator.NotEquals,
                        Operator.In,
                        Operator.NotIn
                    }.Select(op => op.ToString()).ToList();
                }

                if (type == typeof(bool))
                {
                    return new List<Operator>()
                    {
                        Operator.Equals,
                        Operator.NotEquals
                    }.Select(op => op.ToString()).ToList();
                }

                if (Nullable.GetUnderlyingType(type) == typeof(bool))
                {
                    return new List<Operator>()
                    {
                        Operator.Equals,
                        Operator.NotEquals,
                        Operator.IsNull,
                        Operator.NotIsNull
                    }.Select(op => op.ToString()).ToList();
                }

                if (type.IsNumericType() || Nullable.GetUnderlyingType(type)?.IsNumericType() == true)
                {
                    return new List<Operator>()
                    {
                        Operator.Equals,
                        Operator.NotEquals,
                        Operator.GreaterThan,
                        Operator.GreaterOrEqual,
                        Operator.LessThan,
                        Operator.LessOrEqual,
                        Operator.DiffWithinPct
                    }.Select(op => op.ToString()).ToList();
                }

                if (type.IsDateType() || Nullable.GetUnderlyingType(type)?.IsDateType() == true)
                {
                    return new List<Operator>()
                    {
                        Operator.Equals,
                        Operator.NotEquals,
                        Operator.GreaterThan,
                        Operator.GreaterOrEqual,
                        Operator.LessThan,
                        Operator.LessOrEqual
                    }.Select(op => op.ToString()).ToList();
                }
            }

            if (type == typeof(string))
            {
                return new List<Operator>()
                {
                    Operator.Equals,
                    Operator.NotEquals,
                    Operator.Contains,
                    Operator.NotContains,
                    Operator.In,
                    Operator.NotIn,
                    Operator.StartsWith,
                    Operator.NotStartsWith
                }.Select(op => op.ToString()).ToList();
            }

            if (type.IsGenericType || type.IsArray)
            {
                var itemType = type.IsArray ? type.GetElementType() : type.GetGenericArguments()[0];
                var collectionOperators = new List<Operator>()
                {
                    Operator.IsEmpty,
                    Operator.NotIsEmpty
                };
                if (itemType == typeof(string) || itemType!.IsEnum)
                {
                    collectionOperators.AddRange(new[]
                    {
                        Operator.Contains,
                        Operator.NotContains,
                        Operator.AllIn,
                        Operator.NotAllIn,
                        Operator.AnyIn,
                        Operator.NotAnyIn,
                        Operator.ContainsAll,
                        Operator.NotContainsAll
                    });
                }

                if (itemType.IsNumericType() || Nullable.GetUnderlyingType(itemType)?.IsNumericType() == true)
                {
                    collectionOperators.AddRange(new []
                    {
                        Operator.AllInRangePct
                    });
                }

                return collectionOperators.Select(op => op.ToString()).ToList();
            }

            if (!type.IsPrimitiveType())
            {
                var extensionMethods = type.GetExtensionMethods().Where(m => m.GetParameters().Length > 1).ToList();
                if (extensionMethods.Count > 0)
                {
                    return extensionMethods.Select(m => m.Name).ToList();
                }
            }

            return new List<Operator>()
            {
                Operator.IsNull,
                Operator.NotIsNull
            }.Select(op => op.ToString()).ToList();
        }
        #endregion

        #region allowed values

        public List<string> GetAllowedValues(Type owner, PropertyInfo prop)
        {
            var allowedValues = propValuesProvider?.GetAllowedValues(owner, prop)?.GetAwaiter().GetResult().ToList();
            if (allowedValues?.Any() == true)
            {
                return allowedValues;
            }

            if (prop.PropertyType.IsEnum)
            {
                return Enum.GetNames(prop.PropertyType).ToList();
            }

            return new List<string>();
        }
        #endregion
    }
}