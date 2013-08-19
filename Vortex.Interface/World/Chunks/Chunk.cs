using System.Collections.Generic;
using System.Linq;

namespace Vortex.Interface.World.Chunks
{
    public delegate void SingleChunkCallback(Chunk chunk);
    public delegate void ChunkBlockCallback(Chunk chunk, short x, short y, short z);
    public delegate void ChunkMeshCallback(Chunk chunk);

    public class Chunk
    {
        public event SingleChunkCallback ChunkUpdated;
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
        private ChunkMesh _mesh;
        public ChunkMesh ChunkMesh 
        {
            get
            {
                if (_dirty)
                {
                    _mesh.ReplaceContents(GenerateChunkMesh());
                    _dirty = false;
                }
                return _mesh;
            }
            
            private set { _mesh = value; }
        }

        //public const float ChunkWorldSize = 16;

        // helper functions for establishing how this chunk is stored
        public bool BlockBased { get { return ChunkBlocks != null; } }
        public bool MeshBased { get { return ChunkMesh != null; } }

        // rather than update the mesh several times if multiple blocks have changed, we just 
        // flag the mesh as dirty and rebuild it if it is...
        private bool _dirty = false;

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
            //TODO
            return new ChunkMesh();
        }

        private void BlocksUpdated(ChunkBlocks blocks, short x, short y, short z)
        {
            if (ChunkBlockUpdated == null)
                return;

            _dirty = true;
            ChunkBlockUpdated(this, x, y, z);
        }

        // TODO
        // we never really handle this well, might want to look into it
        // as the observable area might need to be updated?
        private void MeshUpdated(ChunkMesh mesh)
        {
            if (ChunkMeshUpdated == null)
                return;

            ChunkMeshUpdated(this);
            ChunkUpdated(this);
        }
    }
}
