namespace Vortex.Renderer.WorldRenderers.ShadowedRenderer
{
    struct TriangleKey
    {
        public readonly int ChunkMeshIndex;
        public readonly int TriangleIndex;

        public TriangleKey(int chunkMeshIndex, int triangleIndex)
        {
            ChunkMeshIndex = chunkMeshIndex;
            TriangleIndex = triangleIndex;
        }
    }
}