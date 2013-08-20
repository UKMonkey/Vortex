using Vortex.Interface.Serialisation;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Vortex.Interface.World.Chunks
{
    public class Chunk : IChunk
    {
        public event SingleChunkCallback ChunkChanged;

        // position
        private short? _type;
        public short Type
        {
            get 
            {
                if (_type == null)
                    _type = ChunkFactory.GetInstance().GetChunkType(this);
                return _type.Value;
            }
        }

        public ChunkKey Key { get; set; }

        // Static lights
        public List<ILight> Lights { get; private set; }

        // for mesh based chunks
        public ChunkMesh ChunkMesh { get; private set; }

        // what level is currently observed by the user
        public short LevelOfInterest { get; set; }

        public Chunk()
        {
            Lights = new List<ILight>();
        }

        protected Chunk(ChunkKey key, IEnumerable<ILight> lights)
        {
            Key = key;
            Lights = lights.ToList();
        }

        public Chunk(ChunkKey key, ChunkMesh mesh, IEnumerable<ILight> lights)
            : this(key, lights)
        {
            ChunkMesh = mesh;
            ChunkMesh.ChunkMeshUpdated += MeshUpdated;
        }

        private void MeshUpdated(ChunkMesh mesh)
        {
            if (ChunkChanged != null)
                ChunkChanged(this);
        }

        public byte[] GetFullData()
        {
            var byteStream = new MemoryStream();
            byteStream.Write(ChunkMesh);
            return byteStream.ToArray();
        }

        public byte[] GetDirtyData()
        {
            return GetFullData();
        }

        public void ApplyData(byte[] data)
        {
            var byteStream = new MemoryStream(data);
            var newMesh = byteStream.ReadChunkMesh();
            if (ChunkMesh != null)
            {
                ChunkMesh.ReplaceContents(newMesh);
            }
            else
            {
                ChunkMesh = newMesh;
                ChunkMesh.ChunkMeshUpdated += MeshUpdated;
            }
        }
    }
}
