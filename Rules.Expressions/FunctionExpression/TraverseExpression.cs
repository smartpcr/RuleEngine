// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TraverseExpression.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions.FunctionExpression
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    public class TraverseExpression : FunctionExpression
    {
        private readonly PropertyInfo traverseProp;
        private readonly PropertyInfo uniqueProp;
        private readonly int maxSteps = -1;
        
        public TraverseExpression(Expression target, FunctionName funcName, params string[] args) : base(target, funcName, args)
        {
            if (args == null || args.Length < 2 || args.Length > 3)
            {
                throw new ArgumentException($"function '{funcName}' requires 2 or 3 arguments");
            }

            var propName = args[0];
            traverseProp = Target.Type.GetMappedProperty(propName);
            if (traverseProp == null)
            {
                throw new InvalidOperationException($"Unable to find property '{propName}' from type '{Target.Type.Name}'");
            }
            
            var uniqueFieldName = args[1];
            uniqueProp = Target.Type.GetMappedProperty(uniqueFieldName);
            if (uniqueProp == null)
            {
                throw new InvalidOperationException($"Unable to find property '{uniqueFieldName}' from type '{Target.Type.Name}'");
            }
            
            if (args.Length == 3)
            {
                maxSteps = int.Parse(args[2]);
            }
        }

        public override Expression Build()
        {
			ParameterExpression parentParam = Expression.Parameter(Target.Type, "parent");
			ParameterExpression pathVar = Expression.Variable(typeof(List<string>), "path");
			ParameterExpression currentVar = Expression.Variable(Target.Type, "current");
			ParameterExpression currentStepVar = Expression.Variable(typeof(int), "currentStep");
			
			ParameterExpression haveLoopVar = Expression.Variable(typeof(bool), "haveLoop");
			MethodInfo addMethod = typeof(List<string>).GetMethod("Add");
			var argumentTypes = new[] {typeof(IEnumerable<string>).GenericTypeArguments[0]};
			LabelTarget label = Expression.Label(typeof(List<string>));
	
			var getTraversalPath = Expression.Block(
				new[] { parentParam, pathVar, currentVar, currentStepVar, haveLoopVar },
				Expression.Assign(pathVar, Expression.Constant(new List<string>())),
				Expression.Assign(currentVar, Target),
				Expression.Call(
					pathVar,
					addMethod,
					Expression.Property(currentVar, uniqueProp)),
				Expression.Assign(currentStepVar, Expression.Constant(1)),
				Expression.Assign(haveLoopVar, Expression.Constant(false)),
				Expression.Assign(parentParam, Expression.Property(currentVar, traverseProp)),
				Expression.Loop(
					Expression.IfThenElse(
						Expression.AndAlso(
							Expression.AndAlso(
								Expression.IsFalse(haveLoopVar),
								Expression.NotEqual(parentParam, Expression.Constant(null, typeof(object)))
							),
							Expression.OrElse(
								Expression.LessThanOrEqual(Expression.Constant(maxSteps), Expression.Constant(0)),
								Expression.LessThanOrEqual(currentStepVar, Expression.Constant(maxSteps))
							)
						),
						Expression.Block(
							Expression.Assign(currentVar, parentParam),
							Expression.Assign(parentParam, Expression.Property(currentVar, traverseProp)),
							Expression.IfThenElse(
								Expression.Call(
									typeof(Enumerable), 
									"Contains", 
									argumentTypes, 
									pathVar, 
									Expression.Property(currentVar, uniqueProp)),
								Expression.Block(
									Expression.Assign(haveLoopVar, Expression.Constant(true))),
								Expression.Block(
									Expression.Call(
										pathVar,
										addMethod,
										Expression.Property(currentVar, uniqueProp)),
									Expression.PostIncrementAssign(currentStepVar)
								)
							)
						),
						Expression.Break(label, pathVar)
					), label
				),
				
				pathVar // return
			);

            return getTraversalPath;
        }
    }
}