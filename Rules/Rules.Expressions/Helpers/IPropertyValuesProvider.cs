namespace Rules.Expressions.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading.Tasks;

    public interface IPropertyValuesProvider
    {
        Task<IEnumerable<string>> GetAllowedValues(Type owner, PropertyInfo prop);
    }
}