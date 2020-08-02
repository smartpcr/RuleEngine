namespace Common.GraphDb
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