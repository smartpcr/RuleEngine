// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AnyInCall.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions.OperatorExpression
{
    using System.Linq.Expressions;

    public class AnyInCall : OperatorExpression
    {
        public AnyInCall(Expression leftExpression, Expression rightExpression): base(leftExpression, rightExpression)
        {
            
        }
        
        public override Expression Create()
        {
            throw new System.NotImplementedException();
        }
    }
}