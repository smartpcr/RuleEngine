namespace Repositories
{
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    public class MappedModelAttribute : Attribute
    {
        public MappedModelAttribute(Type modelType)
        {
            ModelType = modelType;
        }

        public Type ModelType { get; set; }
    }
}