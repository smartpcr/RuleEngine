namespace Rules.Expressions.Builders
{
    using System;
    using System.Collections.Generic;

    public class PropertyPath
    {
        public string Path { get; }
        public Type Type { get; }
        public int ArgumentCount { get; set; } = 0;
        public List<string> AllowedValues { get; set; } = new List<string>();

        public PropertyPath(string path, Type type)
        {
            Path = path;
            Type = type;
        }

        public override string ToString()
        {
            return $"{Path} ({Type.Name}, argCount={ArgumentCount})";
        }
    }
}