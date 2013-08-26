using System;
using System.Collections.Generic;
using Vortex.Interface.World.Chunks;

namespace Vortex.World.Chunks
{
    // this probably shouldn't be in interface...
    public class ChunkFactory : IChunkFactory
    {
        public const short InvalidType = -1;

        private readonly Dictionary<short, Type> _idToType = new Dictionary<short, Type>(); 
        private readonly Dictionary<Type, short> _typeToId = new Dictionary<Type, short>();
        private short _lastId = 1;


        public ChunkFactory()
        {
            RegisterChunkType(typeof(MeshOnlyChunk));
            RegisterChunkType(typeof(BlockChunk));
        }

        public void RegisterChunkType(Type chunkType)
        {
            _idToType[_lastId] = chunkType;
            _typeToId[chunkType] = _lastId;

            _lastId++;
        }

        public IChunk GetChunk(short chunkType)
        {
            if (!_idToType.ContainsKey(chunkType))
                return null;

            var type = _idToType[chunkType];
            var constructor = type.GetConstructor(new Type[]{});
            if (constructor == null)
                throw new Exception("Unable to create a new chunk as it doesn't have a public default constructor");
            return (IChunk)(constructor.Invoke(new object[]{}));
        }

        public short GetChunkType(IChunk chunk)
        {
            var type = chunk.GetType();
            if (_typeToId.ContainsKey(type))
                return _typeToId[type];
            return InvalidType;
        }
    }
}
