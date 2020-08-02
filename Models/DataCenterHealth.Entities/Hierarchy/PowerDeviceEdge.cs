// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PowerDeviceEdge.cs" company="Microsoft Corporation">
//   Copyright (c) Microsoft Corporation. All rights reserved
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Entities.Hierarchy
{
    using Common.GraphDb;

    public class PowerDeviceEdge : IGremlinEdge
    {
        public PowerDeviceNode From { get; set; }
        public PowerDeviceNode To { get; set; }
        public AssociationType Association { get; set; }


        public string GetId()
        {
            return $"{From.Id}-{To.Id}";
        }

        public string GetInVertexId()
        {
            return From.Id;
        }

        public string GetOutVertexId()
        {
            return To.Id;
        }

        public string GetLabel()
        {
            return Association.ToString();
        }

        public IGremlinVertex GetInVertex()
        {
            return From;
        }

        public IGremlinVertex GetOutVertex()
        {
            return To;
        }
    }
}
