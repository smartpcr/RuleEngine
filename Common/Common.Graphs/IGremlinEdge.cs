// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IGremlinEdge.cs" company="Microsoft Corporation">
//   Copyright (c) Microsoft Corporation. All rights reserved
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Common.Graphs
{
    public interface IGremlinEdge
    {
        string GetId();
        string GetInVertexId();
        string GetOutVertexId();
        string GetLabel();

        IGremlinVertex GetInVertex();
        IGremlinVertex GetOutVertex();
    }
}
