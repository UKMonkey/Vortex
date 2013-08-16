using System.Collections.Generic;
using System.Linq;

namespace Vortex.Interface.World.Chunks
{
    public delegate void ChunkBlockCallback(Chunk chunk, int x, int y);
    public delegate void ChunkMeshCallback(Chunk chunk);

    public class Chunk
    {
        public event ChunkBlockCallback ChunkBlockUpdated;
        public event ChunkMeshCallback ChunkMeshUpdated;

        // position
        public readonly ChunkKey Key;

        // Static lights
        public List<ILight> Lights { get; private set; }
        
        // for block based chunks - this may or may not be populated
        // if this is available, it can be used to generate the meshes
        public ChunkBlocks ChunkBlocks { get; private set; }

        // for mesh based chunks
        public ChunkMesh ChunkMesh { get; private set; }
        public const float ChunkWorldSize = 16;

        // helper functions for establishing how this chunk is stored
        public bool BlockBased { get { return ChunkBlocks != null; } }
        public bool MeshBased { get { return ChunkMesh != null; } }

        public Chunk(ChunkKey key, ChunkMesh mesh, IEnumerable<ILight> lights)
        {
            Key = key;
            ChunkMesh = mesh;
            Lights = lights.ToList();
            ChunkBlocks = null;

            ChunkMesh.ChunkMeshUpdated += MeshUpdated;
        }


        public Chunk(ChunkKey key, ChunkBlocks blocks, IEnumerable<ILight> lights)
        {
            Key = key;
            ChunkBlocks = blocks;
            Lights = lights.ToList();
            ChunkMesh = GenerateChunkMesh();

            ChunkBlocks.BlocksUpdated += BlocksUpdated;
            ChunkMesh.ChunkMeshUpdated += MeshUpdated;
        }

        private ChunkMesh GenerateChunkMesh()
        {
            return new ChunkMesh();
        }

        private void BlocksUpdated(ChunkBlocks blocks, int x, int y)
        {
            if (ChunkBlockUpdated == null)
                return;

            ChunkBlockUpdated(this, x, y);
            ChunkMesh = GenerateChunkMesh();
        }

        // TODO
        // we never really handle this well, might want to look into it
        // as the observable area might need to be updated?
        private void MeshUpdated(ChunkMesh mesh)
        {
            if (ChunkMeshUpdated == null)
                return;

            ChunkMeshUpdated(this);
        }
    }
}
