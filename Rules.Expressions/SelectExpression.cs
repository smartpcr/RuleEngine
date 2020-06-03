// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SelectExpression.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    public class SelectExpression
    {
        private readonly MethodInspect callInfo;
        private readonly Expression parent;
        private readonly string selectionPath;

        public SelectExpression(Expression parent, string selectionPath)
        {
            this.parent = parent;
            this.selectionPath = selectionPath;
            callInfo = new MethodInspect("Select", parent.Type, typeof(string), typeof(Enumerable));
        }

        public MethodCallExpression Create()
        {
            var itemType = callInfo.TargetType.GetGenericArguments()[0];
            var paramExpression = Expression.Parameter(itemType, "item");
            var propNames = selectionPath.Split(new[] {'.'});
            Expression propExpression = paramExpression;
            foreach (var propName in propNames)
            {
                var prop = propExpression.Type.GetMappedProperty(propName);
                propExpression = Expression.Property(propExpression, prop);
            }

            Expression selectorExpression = Expression.Lambda(propExpression, paramExpression);

            MethodInfo selectMethod = null;
            foreach (var m in typeof(Enumerable).GetMethods().Where(m => m.Name == "Select"))
            foreach (var p in m.GetParameters().Where(p => p.Name.Equals("selector")))
                if (p.ParameterType.GetGenericArguments().Count() == 2)
                    selectMethod = (MethodInfo) p.Member;
            if (selectMethod == null) throw new InvalidOperationException("Failed to get generic select method");

            var genericSelectMethod = selectMethod.MakeGenericMethod(itemType, propExpression.Type);
            var selectExpression = Expression.Call(
                null,
                genericSelectMethod,
                parent,
                selectorExpression);

            return selectExpression;
        }

    }
}