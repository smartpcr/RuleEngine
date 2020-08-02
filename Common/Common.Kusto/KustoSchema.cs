// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KustoSchema.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Common.Kusto
{
    using System;
    using System.Collections.Generic;
    using System.Reflection.Metadata.Ecma335;
    using System.Text;

    public class KustoColumn
    {
        public string Name { get; set; }
        public Type Type { get; set; }
        public string CslType { get; set; }
    }

    public class KustoTable
    {
        public string Name { get; set; }
        public List<KustoColumn> Columns { get; set; }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append($".create-merge table {Name} (\n");
            for(var i = 0; i < Columns.Count; i++)
            {
                var col = Columns[i];
                stringBuilder.Append($"{col.Name}:{col.CslType}");
                if (i < Columns.Count - 1)
                {
                    stringBuilder.Append(",\n");
                }
                else
                {
                    stringBuilder.Append(")");
                }
            }

            return stringBuilder.ToString();
        }
    }

    public class KustoFunction
    {
        public string Name { get; set; }
        public string Parameters { get; set; }
        public string Body { get; set; }
        public string Folder { get; set; }
        public string DocString { get; set; }

        public override string ToString()
        {
            return $".create-or-alter function with (folder = \"{Folder}\", docstring = \"{DocString}\") {Name}{Parameters}" +
                   "\n" + Body + "\n";
        }
    }
}