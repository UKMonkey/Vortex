namespace Vortex
{
    public interface IRenderResult
    {
        int VertexRenderCount { get; }
        int StateChanges { get; }
    }
}