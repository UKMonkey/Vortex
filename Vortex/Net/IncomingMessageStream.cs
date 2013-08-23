using System.Collections.Generic;
using Lidgren.Network;
using SlimMath;
using Vortex.Interface;
using Vortex.Interface.EntityBase;
using Vortex.Interface.EntityBase.Properties;
using Vortex.Interface.Net;
using Vortex.Interface.Traits;
using Vortex.Interface.World;
using Vortex.Interface.World.Chunks;

namespace Vortex.Net
{
    public class IncomingMessageStream : IIncomingMessageStream
    {
        private readonly NetIncomingMessage _netIncomingMessage;
        private readonly IEngine _engine;

        public IncomingMessageStream(
            NetIncomingMessage netIncomingMessage,
            IEngine engine)
        {
            _netIncomingMessage = netIncomingMessage;
            _engine = engine;
        }

        public RemotePlayer RemoteClient()
        {
            return (RemotePlayer)_netIncomingMessage
                .SenderConnection.Tag;
        }

        public List<RemotePlayer> ReadRemotePlayers()
        {
            var count = _netIncomingMessage.ReadByte();
            var result = new List<RemotePlayer>(count);
            for (var i = 0; i < count; i++)
            {
                result.Add(ReadRemotePlayer());
            }

            return result;
        }

        public RemotePlayer ReadRemotePlayer()
        {
            var playerId = _netIncomingMessage.ReadUInt16();
            var playerName = ReadString();
            return new RemotePlayer(playerId, playerName);
        }

        public byte ReadByte()
        {
            return _netIncomingMessage.ReadByte();
        }

        public byte[] ReadBytes()
        {
            var size = _netIncomingMessage.ReadInt16();
            return _netIncomingMessage.ReadBytes(size);
        }

        public short ReadInt16()
        {
            return _netIncomingMessage.ReadInt16();
        }

        public ushort ReadUint16()
        {
            return _netIncomingMessage.ReadUInt16();
        }

        public int ReadInt32()
        {
            return _netIncomingMessage.ReadInt32();
        }

        public uint ReadUInt32()
        {
            return _netIncomingMessage.ReadUInt32();
        }

        public long ReadInt64()
        {
            return _netIncomingMessage.ReadInt64();
        }

        public string ReadString()
        {
            return _netIncomingMessage.ReadString();
        }

        public List<string> ReadStringList()
        {
            var count = _netIncomingMessage.ReadByte();
            var result = new List<string>(count);

            for (int i = 0; i < count; i++)
            {
                result.Add(ReadString());
            }

            return result;
        }

        public float ReadFloat()
        {
            return _netIncomingMessage.ReadFloat();
        }

        public double ReadDouble()
        {
            return _netIncomingMessage.ReadDouble();
        }

        public Vector3 ReadVector()
        {
            var x = _netIncomingMessage.ReadSingle();
            var y = _netIncomingMessage.ReadSingle();
            var z = _netIncomingMessage.ReadSingle();
            return new Vector3(x, y, z);
        }

        public float ReadRotation()
        {
            return _netIncomingMessage.ReadRangedSingle(-12, 12, 10);
        }

        public int ReadEntityId()
        {
            return _netIncomingMessage.ReadInt32();
        }

        public bool ReadBoolean()
        {
            return _netIncomingMessage.ReadBoolean();
        }

        public List<Entity> ReadEntities()
        {
            var count = _netIncomingMessage.ReadInt16();
            var ret = new List<Entity>(count);
            for(var i=0; i<count; ++i)
                ret.Add(ReadEntity());
            return ret;
        }

        public Color4 ReadColour()
        {
            var a = _netIncomingMessage.ReadByte();
            var r = _netIncomingMessage.ReadByte();
            var g = _netIncomingMessage.ReadByte();
            var b = _netIncomingMessage.ReadByte();
            return new Color4(a, r, g, b);
        }

        public List<ILight> ReadLights()
        {
            var count = _netIncomingMessage.ReadInt16();
            var lights = new List<ILight>(count);
            for (var i = 0; i < count; i++)
            {
                lights.Add(ReadLight());
            }
            return lights;
        }

        public ILight ReadLight()
        {
            var position = ReadVector();
            var brightness = _netIncomingMessage.ReadFloat();
            var colour = ReadColour();
            var isDynamic = _netIncomingMessage.ReadBoolean();
            var enabled = _netIncomingMessage.ReadBoolean();

            return new Light(position, brightness, colour)
                   {
                       Enabled = enabled, 
                       IsDynamic = isDynamic
                   };
        }

        public List<int> ReadEntityIds()
        {
            var count = _netIncomingMessage.ReadInt16();
            var ids = new List<int>(count);

            for (var i = 0; i < count; i++)
            {
                ids.Add(ReadEntityId());
            }

            return ids;
        }

        public List<Entity> ReadEntityList()
        {
            var count = _netIncomingMessage.ReadInt16();
            var ret = new List<Entity>(count);

            for (var i = 0; i < count; ++i)
            {
                ret.Add(ReadEntity());
            }
            return ret;
        }

        public Entity ReadEntity()
        {
            var entityId = ReadEntityId();
            var entityType = _netIncomingMessage.ReadInt16();
            var parentEntityId = ReadEntityId();
            var nonDefProperties = ReadTraits<EntityProperty>();

            var newEntity = _engine.EntityFactory.Get(entityType);
            newEntity.Parent = parentEntityId;
            newEntity.EntityId = entityId;
            newEntity.SetProperties(nonDefProperties);

            return newEntity;
        }

        public List<IChunk> ReadChunks()
        {
            var count = _netIncomingMessage.ReadByte();
            var result = new List<IChunk>(count);
            for (var i = 0; i < count; i++)
            {
                result.Add(ReadChunk());
            }
            return result;
        }

        public IChunk ReadChunk()
        {
            var chunkKey = ReadChunkKey();
            var type = ReadInt16();
            var data = ReadBytes();
            var lights = ReadLights();

            var chunk = _engine.ChunkFactory.GetChunk(type);
            chunk.ApplyFullData(data);
            chunk.Lights.AddRange(lights);
            chunk.Key = chunkKey;
            return chunk;
        }

        public ChunkKey ReadChunkKey()
        {
            var x = ReadInt32();
            var y = ReadInt32();
            return new ChunkKey(x, y);
        }

        public List<ChunkKey> ReadChunkKeys()
        {
            var count = _netIncomingMessage.ReadByte();
            var result = new List<ChunkKey>(count);
            for (int i = 0; i < count; i++)
            {
                result.Add(ReadChunkKey());
            }
            return result;
        }

        public T ReadTrait<T>() where T : Trait, new()
        {
            var propertyId = _netIncomingMessage.ReadInt16();
            var size = _netIncomingMessage.ReadInt16();
            var data = _netIncomingMessage.ReadBytes(size);

            var thing = new T {PropertyId = propertyId, ByteArrayValue = data};

            return thing;
        }

        public List<T> ReadTraits<T>() where T:Trait, new()
        {
            var count = ReadInt16();
            var ret = new List<T>(count);
            for (var i=0; i<count; ++i)
                ret.Add(ReadTrait<T>());
            return ret;
        }

        public List<T> ReadEntityProperties<T>() where T : Trait, new()
        {
            var count = _netIncomingMessage.ReadInt16();
            var ret = new List<T>(count);

            for (var i = 0; i < count; ++i)
            {
                ret.Add(ReadTrait<T>());
            }

            return ret;
        }

        public T2 Read<T1, T2>(IIncomingMessageStream messageStream)
            where T1 : Trait, new()
            where T2 : TraitCollection<T1>, new()
        {
            var ret = new T2();
            var traits = ReadTraits<T1>();

            ret.SetProperties(traits);

            return ret;
        }
    }
}