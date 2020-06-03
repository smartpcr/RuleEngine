namespace Rules.Expressions
{
    using System;

    public class MethodCheck
    {
        public MethodCheck(string methodName, Predicate<Type> targetTypeCheck, Predicate<Type> argumentTypeCheck)
        {
            MethodName = methodName;
            TargetTypeCheck = targetTypeCheck;
            ArgumentTypeCheck = argumentTypeCheck;
        }

        public string MethodName { get; }
        public Predicate<Type> TargetTypeCheck { get; }
        public Predicate<Type> ArgumentTypeCheck { get; }

        public bool CheckExpression(string methodName, Type targetType, object argumentValue)
        {
            var methodCheck = MethodName == methodName;
            var targetTypeCheck = TargetTypeCheck(targetType);
            var valueTypeCheck = ArgumentTypeCheck(argumentValue.GetType());

            return methodCheck && targetTypeCheck && valueTypeCheck;
        }
    }
}