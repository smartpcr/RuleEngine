// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IOperatorCallExpression.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions.OperatorExpression
{
    using System.Linq.Expressions;

    public interface IOperatorExpression
    {
        Expression Create();
    }

    public abstract class OperatorExpression : IOperatorExpression
    {
        protected Expression LeftExpression { get; set; }
        protected Expression RightExpression { get; set; }
        
        protected OperatorExpression(Expression leftExpression, Expression rightExpression)
        {
            LeftExpression = leftExpression;
            RightExpression = rightExpression;
        }
        
        public abstract Expression Create();
    }
}