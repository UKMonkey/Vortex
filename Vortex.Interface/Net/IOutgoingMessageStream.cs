using System.Collections.Generic;
using SlimMath;
using Vortex.Interface.EntityBase.Properties;
using Psy.Core;
using Vortex.Interface.EntityBase;
using Vortex.Interface.Traits;
using Vortex.Interface.World;
using Vortex.Interface.World.Chunks;

namespace Vortex.Interface.Net
{
    public interface IOutgoingMessageStream
    {
        void WriteRotation(float rotation);

        void WriteBool(bool value);
        void WriteByte(byte value);
        void WriteBytes(byte[] value);
        void WriteBytes(byte[] value, int size);
        void WriteInt16(short value);
        void WriteUInt16(ushort clientId);
        void WriteInt32(int value);
        void WriteUInt32(uint value);
        void WriteInt64(long expireTime);
        void WriteFloat(float value);
        void WriteDouble(double value);
        void WriteEntityId(int entityId);

        void Write(string value);
        void Write(RemotePlayer remotePlayer);
        void Write(Color4 colour);
        void Write(ILight light);
        void Write(Chunk chunk);
        void Write(ChunkKey chunkKey);
        void Write(Vector3 chunk);
        void Write<T>(T property) where T : Trait;

        void Write(Entity entity);
        void Write(List<string> strings);
        void Write(List<RemotePlayer> remotePlayers);
        void Write(List<short> shorts);
        void Write(List<int> ints);
        void Write(List<Chunk> chunks);
        void Write(List<ILight> lights);
        void Write(List<Entity> entities);
        void Write(List<ChunkKey> chunkKeys);
        void Write<T>(List<T> properties) where T : Trait;
    }
}