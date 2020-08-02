// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PropertyPathExtension.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using Common.Config;
    using DataCenterHealth.Models.Devices;

    public class PropertyPathBuilder<T> where T: class
    {
        private static readonly Regex PartSplitter = new Regex(@"\.(?![^(]*\))", RegexOptions.Compiled); // only matches dot (.) outside parenthesis
        private readonly IPropertyExpression propExpand;
        private readonly IPropertyValuesProvider propValuesProvider;
        private readonly List<FunctionName> aggregateFunctions;
        private readonly int maxDepth;
        private readonly int maxDuplicates;
        private readonly object syncLock = new object();
        private static List<PropertyPath> allPropPaths;

        public PropertyPathBuilder(
            IPropertyExpression propertyExpand,
            IPropertyValuesProvider propValuesProvider,
            int maxDepth = 4, int maxDuplicates = 2)
        {
            propExpand = propertyExpand;
            this.propValuesProvider = propValuesProvider;
            this.maxDepth = maxDepth;
            this.maxDuplicates = maxDuplicates;
            aggregateFunctions=new List<FunctionName>()
            {
                FunctionName.Count,
                FunctionName.DistinctCount,
                FunctionName.Max,
                FunctionName.Min,
                FunctionName.Average,
                FunctionName.Sum
            };
        }

        public List<PropertyPath> AllPropPaths
        {
            get
            {
                if (allPropPaths == null || allPropPaths.Count == 0)
                {
                    lock (syncLock)
                    {
                        if (allPropPaths == null || allPropPaths.Count == 0)
                        {
                            allPropPaths = new List<PropertyPath>();
                            allPropPaths.Add(new PropertyPath("", typeof(T)));
                            allPropPaths.Add(new PropertyPath("Traverse(PrimaryParentDevice, DeviceName)", typeof(IEnumerable<string>)));
                            allPropPaths.Add(new PropertyPath("Traverse(PrimaryParentDevice, DeviceName).Count()", typeof(int)));
                            allPropPaths.Add(new PropertyPath("Traverse(PrimaryParentDevice, DeviceName).Last()", typeof(string)));
                            BFS("", typeof(T), allPropPaths);
                        }
                    }
                }

                return allPropPaths;
            }
        }

        public List<PropertyPath> GetAllFieldParts(string current)
        {
            if (string.IsNullOrEmpty(current))
            {
                var firstParts = AllPropPaths.ToList();
                return firstParts;
            }

            var nextParts = AllPropPaths.Where(pt => pt.Path.StartsWith(current)).ToList();
            return nextParts;
        }

        public List<PropertyPath> GetRootFieldParts()
        {
            var rootParts = AllPropPaths.Where(pt => PartSplitter.Split(pt.Path).Length == 1).ToList();
            return rootParts;
        }

        public List<PropertyPath> GetNextFieldPart(string current)
        {
            if (string.IsNullOrEmpty(current))
            {
                var firstParts = AllPropPaths.Where(pt => PartSplitter.Split(pt.Path).Length == 1).ToList();
                return firstParts;
            }

            var currentDepth = PartSplitter.Split(current).Length;
            var nextParts = AllPropPaths.Where(pt => pt.Path.StartsWith(current, StringComparison.OrdinalIgnoreCase))
                .Where(pt => pt.Path != current && PartSplitter.Split(pt.Path).Length <= currentDepth + 1)
                .ToList();
            return nextParts;
        }

        #region fields


        private void BFS(string currentPath, Type currentType, List<PropertyPath> existingPaths, bool scalarPropsOnly = false)
        {
            if (currentType.IsPrimitiveType() || currentType == typeof(string))
            {
                return;
            }

            if (PartSplitter.Split(currentPath).Length > maxDepth) return;

            var iteration = new Queue<(string path, Type type, bool scalarPropsOnly)>();

            if (currentType.IsArray || currentType.IsGenericType)
            {
                var elementType = currentType.IsArray
                    ? currentType.GetElementType()
                    : currentType.GetGenericArguments()[0];
                if (elementType == null) return;

                ExpandObjectCollectionFunctions(currentPath, elementType, existingPaths);
                ExpandObjectCollectionItemFunctions(currentPath, elementType, existingPaths, scalarPropsOnly);
            }
            else
            {
                var childProps = currentType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => propExpand.CanQuery(currentType, p));
                if (scalarPropsOnly)
                {
                    childProps = childProps.Where(p => p.PropertyType.IsPrimitiveType() || p.PropertyType == typeof(string)).ToArray();
                }

                foreach (var childProp in childProps)
                {
                    var childPath = string.IsNullOrEmpty(currentPath) ? childProp.Name : $"{currentPath}.{childProp.Name}";
                    existingPaths.Add(new PropertyPath(childPath, childProp.PropertyType));

                    if (!childProp.PropertyType.IsPrimitiveType() && childProp.PropertyType != typeof(string))
                    {
                        var (dupTypeCount, dupPropCount) = CheckDuplication(existingPaths, childPath, childProp.PropertyType);
                        if (dupTypeCount < maxDuplicates && dupPropCount < maxDuplicates)
                        {
                            iteration.Enqueue((childPath, childProp.PropertyType, scalarPropsOnly && dupPropCount > 1));
                        }
                    }
                }

                var macroMethods = propExpand.GetMacroMethods(currentType);
                if (macroMethods.Any())
                {
                    if (scalarPropsOnly)
                    {
                        macroMethods = macroMethods.Where(p => p.ReturnType.IsPrimitiveType() || p.ReturnType == typeof(string)).ToList();
                    }

                    foreach (var macro in macroMethods)
                    {
                        var childPath = string.IsNullOrEmpty(currentPath) ? $"{macro.Name}()" : $"{currentPath}.{macro.Name}()";
                        existingPaths.Add(new PropertyPath(childPath, macro.ReturnType));

                        if (!macro.ReturnType.IsPrimitiveType() && macro.ReturnType != typeof(string))
                        {
                            var (dupTypeCount, dupPropCount) = CheckDuplication(existingPaths, childPath, macro.ReturnType);
                            if (dupTypeCount < maxDuplicates && dupPropCount < maxDuplicates)
                            {
                                iteration.Enqueue((childPath, macro.ReturnType, scalarPropsOnly && dupPropCount > 1));
                            }
                        }
                    }
                }
            }

            while (iteration.Count > 0)
            {
                var (nextPath, nextType, useScalarProps) = iteration.Dequeue();
                BFS(nextPath, nextType, existingPaths, scalarPropsOnly && useScalarProps);
            }
        }

        private void ExpandNumericFunctions(string currentPath, Type itemType, List<PropertyPath> existingPaths)
        {
            existingPaths.AddRange(aggregateFunctions.Select(f => new PropertyPath(string.IsNullOrEmpty(currentPath) ? $"{f.ToString()}()" : $"{currentPath}.{f.ToString()}()", itemType)));
            existingPaths.Add(new PropertyPath($"{currentPath}.{FunctionName.OrderBy}()", typeof(IEnumerable<>).MakeGenericType(itemType)));
            existingPaths.Add(new PropertyPath($"{currentPath}.{FunctionName.OrderByDesc}()", typeof(IEnumerable<>).MakeGenericType(itemType)));
        }

        private void ExpandObjectCollectionFunctions(string currentPath, Type itemType, List<PropertyPath> existingPaths)
        {
            existingPaths.Add(new PropertyPath(string.IsNullOrEmpty(currentPath) ? $"{FunctionName.Count}()" : $"{currentPath}.{FunctionName.Count}()", typeof(int)));
            existingPaths.Add(new PropertyPath(string.IsNullOrEmpty(currentPath) ? $"{FunctionName.First}()" : $"{currentPath}.{FunctionName.First}()", itemType));
            existingPaths.Add(new PropertyPath(string.IsNullOrEmpty(currentPath) ? $"{FunctionName.Last}()" : $"{currentPath}.{FunctionName.Last}()", itemType));
        }

        private void ExpandObjectCollectionItemFunctions(string currentPath, Type itemType, List<PropertyPath> existingPaths, bool scalarPropsOnly = false)
        {
            if (itemType.IsPrimitiveType())
            {
                if (itemType.IsNumericType() || Nullable.GetUnderlyingType(itemType)?.IsNumericType() == true)
                {
                    ExpandNumericFunctions(currentPath, itemType, existingPaths);
                }
            }
            else if (itemType == typeof(string))
            {
                ExpandStringCollectionFunctions(currentPath, itemType, existingPaths);
            }
            else if (itemType.IsArray || itemType.IsGenericType)
            {
                var childElementType = itemType.IsArray
                    ? itemType.GetElementType()
                    : itemType.GetGenericArguments()[0];
                if (childElementType == null) return;

                // select many
                var selectManyChildPath = string.IsNullOrEmpty(currentPath) ? $"{FunctionName.SelectMany}()" : $"{currentPath}.{FunctionName.SelectMany}()";
                existingPaths.Add(new PropertyPath(selectManyChildPath, typeof(IEnumerable<>).MakeGenericType(childElementType)));
            }

            var childProps = itemType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => propExpand.CanQuery(itemType, p));
            var iteration = new Queue<(string path, Type type, bool scalarPropsOnly)>();
            var currentParts = PartSplitter.Split(currentPath ?? "");
            var lastPart = currentParts[currentParts.Length -1];

            if (scalarPropsOnly)
            {
                childProps = childProps.Where(p => p.PropertyType.IsPrimitiveType() || p.PropertyType == typeof(string)).ToArray();
            }
            foreach (var childProp in childProps)
            {
                // selectMany
                if ((childProp.PropertyType.IsArray || childProp.PropertyType.IsGenericType) &&
                    !childProp.PropertyType.IsPrimitiveType() &&
                    currentParts.Length < 2)
                {
                    var childPropElementType = childProp.PropertyType.IsArray
                        ? childProp.PropertyType.GetElementType()
                        : childProp.PropertyType.GetGenericArguments()[0];
                    if (childPropElementType == null) return;

                    // select many
                    var selectManyChildPath = $"{currentPath}.{FunctionName.SelectMany}({childProp.Name})";
                    var selectManyChildType = typeof(IEnumerable<>).MakeGenericType(childPropElementType);
                    existingPaths.Add(new PropertyPath(selectManyChildPath, selectManyChildType));
                    var (dupTypeCount, dupPropCount) = CheckDuplication(existingPaths, selectManyChildPath, selectManyChildType);
                    if (dupTypeCount < maxDuplicates && dupPropCount < maxDuplicates)
                    {
                        iteration.Enqueue((selectManyChildPath, selectManyChildType, scalarPropsOnly && dupPropCount > 1));
                    }
                }

                // orderby
                if (propExpand.CanSort(itemType, childProp))
                {
                    if (!lastPart.StartsWith(FunctionName.OrderBy.ToString()) && !lastPart.StartsWith(FunctionName.OrderByDesc.ToString()))
                    {
                        if (childProp.PropertyType.IsPrimitiveType() || childProp.PropertyType == typeof(string))
                        {
                            var childPath = $"{currentPath}.{FunctionName.OrderBy}({childProp.Name})";
                            existingPaths.Add(new PropertyPath(childPath, typeof(IEnumerable<>).MakeGenericType(itemType)));
                            var (dupTypeCount, dupPropCount) = CheckDuplication(existingPaths, childPath, childProp.PropertyType);
                            if (dupTypeCount < maxDuplicates && dupPropCount < maxDuplicates)
                            {
                                iteration.Enqueue((childPath, typeof(IEnumerable<>).MakeGenericType(itemType), scalarPropsOnly && dupPropCount > 1));
                            }

                            childPath = $"{currentPath}.{FunctionName.OrderByDesc}({childProp.Name})";
                            existingPaths.Add(new PropertyPath(childPath, typeof(IEnumerable<>).MakeGenericType(itemType)));
                            (dupTypeCount, dupPropCount) = CheckDuplication(existingPaths, childPath, childProp.PropertyType);
                            if (dupTypeCount < maxDuplicates && dupPropCount < maxDuplicates)
                            {
                                iteration.Enqueue((childPath, typeof(IEnumerable<>).MakeGenericType(itemType), scalarPropsOnly && dupPropCount > 1));
                            }
                        }
                    }
                }

                // select
                if (propExpand.CanSelect(itemType, childProp))
                {
                    var selectChildPath = string.IsNullOrEmpty(currentPath) ? $"{FunctionName.Select}({childProp.Name})" : $"{currentPath}.{FunctionName.Select}({childProp.Name})";
                    existingPaths.Add(new PropertyPath(selectChildPath, typeof(IEnumerable<>).MakeGenericType(childProp.PropertyType)));
                    var (dupTypeCount1, dupPropCount1) = CheckDuplication(existingPaths, selectChildPath, childProp.PropertyType);
                    if (dupTypeCount1 < maxDuplicates && dupPropCount1 < maxDuplicates)
                    {
                        iteration.Enqueue((selectChildPath, typeof(IEnumerable<>).MakeGenericType(childProp.PropertyType), scalarPropsOnly && dupPropCount1 > 1));
                    }
                }

                // where
                if (propExpand.CanCompare(itemType, childProp) && !lastPart.StartsWith(FunctionName.Where.ToString()))
                {
                    var availableValues = GetAllowedValues(itemType, childProp);
                    if (availableValues != null && availableValues.Count > 0)
                    {
                        foreach (var value in availableValues)
                        {
                            var whereChildPath = string.IsNullOrEmpty(currentPath) ? $"{FunctionName.Where}({childProp.Name}, Equals, '{value}')" :$"{currentPath}.Where({childProp.Name}, Equals, '{value}')";
                            existingPaths.Add(new PropertyPath(whereChildPath, typeof(IEnumerable<>).MakeGenericType(itemType)));
                            var (dupTypeCount2, dupPropCount2) = CheckDuplication(existingPaths, whereChildPath, childProp.PropertyType);
                            if (dupTypeCount2 < maxDuplicates && dupPropCount2 < maxDuplicates)
                            {
                                iteration.Enqueue((whereChildPath, typeof(IEnumerable<>).MakeGenericType(itemType), scalarPropsOnly && dupPropCount2 > 1));
                            }
                        }
                    }
                }

                // aggregate
                if (childProp.PropertyType.IsNumericType() || Nullable.GetUnderlyingType(childProp.PropertyType)?.IsNumericType() == true)
                {
                    existingPaths.Add(new PropertyPath($"{currentPath}.{FunctionName.Average}({childProp.Name})", childProp.PropertyType));
                    existingPaths.Add(new PropertyPath($"{currentPath}.{FunctionName.Min}({childProp.Name})", childProp.PropertyType));
                    existingPaths.Add(new PropertyPath($"{currentPath}.{FunctionName.Max}({childProp.Name})", childProp.PropertyType));
                    existingPaths.Add(new PropertyPath($"{currentPath}.{FunctionName.Sum}({childProp.Name})", childProp.PropertyType));
                }
            }

            while (iteration.Count > 0)
            {
                var (nextPath, nextType, useScalarProps) = iteration.Dequeue();
                BFS(nextPath, nextType, existingPaths, scalarPropsOnly && useScalarProps);
            }
        }

        private void ExpandStringCollectionFunctions(string currentPath, Type itemType, List<PropertyPath> existingPaths)
        {
            existingPaths.Add(new PropertyPath(string.IsNullOrEmpty(currentPath) ? $"{FunctionName.DistinctCount}()" : $"{currentPath}.{FunctionName.DistinctCount}()", typeof(int)));
            existingPaths.Add(new PropertyPath(string.IsNullOrEmpty(currentPath) ? $"{FunctionName.OrderBy}()" : $"{currentPath}.{FunctionName.OrderBy}()", itemType));
            existingPaths.Add(new PropertyPath(string.IsNullOrEmpty(currentPath) ? $"{FunctionName.OrderByDesc}()" : $"{currentPath}.{FunctionName.OrderByDesc}()", typeof(IEnumerable<>).MakeGenericType(itemType)));
        }

        private (int dupTypeCount, int dupPropCount) CheckDuplication(List<PropertyPath> existingPaths, string currentPath, Type currentType)
        {
            var currentParts = PartSplitter.Split(currentPath);

            var ancestorPaths = Enumerable.Range(1, currentParts.Length).Select(size => string.Join(".", currentParts.Take(size))).ToList();
            var dupTypeCount = currentType.IsPrimitiveType() || currentType == typeof(string)
                ? 0
                : existingPaths.Count(pt => pt.Type == currentType && ancestorPaths.Contains(pt.Path, StringComparer.OrdinalIgnoreCase));

            var currentPart = currentParts[currentParts.Length - 1];
            foreach (var funcName in Enum.GetNames(typeof(FunctionName)))
            {
                if (currentPart.StartsWith(funcName))
                {
                    currentPart = funcName;
                    break;
                }
            }
            var dupPropCount = currentParts.Count(part => part.StartsWith(currentPart, StringComparison.OrdinalIgnoreCase));
            return (dupTypeCount, dupPropCount);
        }
        #endregion

        #region operators

        public List<string> GetApplicableOperators(string propPath)
        {
            var partType = AllPropPaths.FirstOrDefault(pt => pt.Path == propPath);
            if (partType?.Path != null)
            {
                return GetOperatorsForType(partType.Type);
            }

            return Enum.GetNames(typeof(Operator)).ToList();
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
                if (itemType == typeof(string) || itemType.IsEnum)
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

            if (type == typeof(PowerDevice))
            {
                var extensionMethods = type.GetExtensionMethods()
                    .Where(m => m.GetParameters().Length > 1).ToList();
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

        public List<FunctionName> GetAllowedFunctionsForPropValue(string propPath)
        {
            var partType = AllPropPaths.FirstOrDefault(pt => pt.Path == propPath);
            if (partType.Path != null)
            {
                if (partType.Type.IsDateType() || Nullable.GetUnderlyingType(partType.Type)?.IsDateType() == true)
                {
                    return new List<FunctionName>()
                    {
                        FunctionName.Ago
                    };
                }
            }

            return new List<FunctionName>();
        }

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

    public class PropertyPath
    {
        public string Path { get; }
        public Type Type { get; }

        public PropertyPath(string path, Type type)
        {
            Path = path;
            Type = type;
        }
    }
}