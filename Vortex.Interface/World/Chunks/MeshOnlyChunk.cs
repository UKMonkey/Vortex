using Vortex.Interface.Serialisation;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Vortex.Interface.World.Chunks
{
    public class MeshOnlyChunk : IChunk
    {
        public event SingleChunkCallback ChunkChanged;

        public ChunkKey Key { get; set; }
        public List<ILight> Lights { get; private set; }

        private ChunkMesh ChunkMesh { get; set; }

        public MeshOnlyChunk()
        {
            Lights = new List<ILight>();
        }

        protected MeshOnlyChunk(ChunkKey key, IEnumerable<ILight> lights)
        {
            Key = key;
            Lights = lights.ToList();
        }

        public MeshOnlyChunk(ChunkKey key, ChunkMesh mesh, IEnumerable<ILight> lights)
            : this(key, lights)
        {
            ChunkMesh = mesh;
        }

        private void HandleChunkChanged()
        {
            if (ChunkChanged != null)
                ChunkChanged(this);
        }

        // do nothing - the mesh has already been calculated
        public void RecalculateMesh(IEngine engine)
        {
        }

        public ChunkMesh GetChunkMesh(int levelOfInterest)
        {
            return ChunkMesh;
        }

        public IEnumerable<KeyValuePair<int, ChunkMesh>> GetChunkMeshes()
        {
            yield return new KeyValuePair<int, ChunkMesh>(0, ChunkMesh);
        }

        public byte[] GetFullData()
        {
            var byteStream = new MemoryStream();
            byteStream.Write(ChunkMesh);
            return byteStream.ToArray();
        }

        public void ApplyFullData(byte[] data)
        {
            var byteStream = new MemoryStream(data);
            ChunkMesh = byteStream.ReadChunkMesh();
            HandleChunkChanged();
        }

        public byte[] GetDirtyData()
        {
            return GetFullData();
        }

        public void ApplyDirtyData(byte[] data)
        {
            ApplyFullData(data);
        }
    }
}
