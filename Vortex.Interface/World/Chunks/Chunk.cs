using System.Collections.Generic;

namespace Vortex.Interface.World.Chunks
{
    public class Chunk
    {
        public readonly ChunkKey Key;

        public ChunkMesh ChunkMesh { get; private set; }

        // Static lights
        public List<ILight> Lights { get; private set; }
        public const float ChunkWorldSize = 16;

        public Chunk(ChunkKey key, ChunkMesh mesh, List<ILight> lights)
        {
            Key = key;
            ChunkMesh = mesh;
            Lights = lights;
        }
    }
}
