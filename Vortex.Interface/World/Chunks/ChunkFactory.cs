using System;
using System.Collections.Generic;

namespace Vortex.Interface.World.Chunks
{
    // this probably shouldn't be in interface...
    public class ChunkFactory
    {
        private static readonly ChunkFactory Instance = new ChunkFactory();

        public const short InvalidType = -1;

        private readonly Dictionary<short, Type> _idToType = new Dictionary<short, Type>(); 
        private readonly Dictionary<Type, short> _typeToId = new Dictionary<Type, short>();
        private short _lastId = 1;


        public static ChunkFactory GetInstance()
        {
            return Instance;
        }

        private ChunkFactory()
        {
            RegisterType(typeof (Chunk));
        }

        private void RegisterType(Type chunkType)
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
