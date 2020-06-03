// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Calculator_feature_steps.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions.Tests.Example
{
    using System;
    using LightBDD.Framework.Parameters;
    using LightBDD.MsTest2;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public class Calculator
    {
        /// <summary>
        /// This is an example of flawed logic that would cause some of the tests to fail.
        /// </summary>
        public int Add(int x, int y)
        {
            return x + y;
        }

        public int Multiply(int x, int y)
        {
            return x * y;
        }

        public int Divide(int x, int y)
        {
            return x / y;
        }
    }

    public partial class Calculator_feature : FeatureFixture
    {
        private Calculator _calculator;
        
        private void Given_a_calculator()
        {
            _calculator = new Calculator();
        }

        private void Then_adding_X_to_Y_should_give_RESULT(int x, int y, Verifiable<int> result)
        {
            result.SetActual(() => _calculator.Add(x, y));
        }

        private void Then_dividing_by_0_should_throw(int x, int y)
        {
            Assert.ThrowsException<DivideByZeroException>(() => _calculator.Divide(x, y));
        }

        private void Then_dividing_X_by_Y_should_give_RESULT(int x, int y, Verifiable<int> result)
        {
            result.SetActual(() => _calculator.Divide(x, y));
        }
        
        private void Then_multiplying_X_by_Y_should_give_RESULT(int x, int y, Verifiable<int> result)
        {
            if (x < 0 || y < 0)
                Assert.Inconclusive("Negative numbers are not supported yet");
            result.SetActual(() => _calculator.Multiply(x, y));
        }
    }
}