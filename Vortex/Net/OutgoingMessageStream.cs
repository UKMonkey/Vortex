using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lidgren.Network;
using SlimMath;
using Vortex.Interface.EntityBase;
using Vortex.Interface.EntityBase.Properties;
using Vortex.Interface.Net;
using Vortex.Interface.Traits;
using Vortex.Interface.World;
using Vortex.Interface.World.Chunks;

namespace Vortex.Net
{
    public class OutgoingMessageStream : IOutgoingMessageStream
    {
        private readonly NetOutgoingMessage _netOutgoingMessage;

        public OutgoingMessageStream(NetOutgoingMessage netOutgoingMessage)
        {
            _netOutgoingMessage = netOutgoingMessage;
        }

        public void Write(RemotePlayer remotePlayer)
        {
            _netOutgoingMessage.Write((ushort)remotePlayer.ClientId);
            _netOutgoingMessage.Write(remotePlayer.PlayerName);
        }

        public void Write(List<RemotePlayer> remotePlayers)
        {
            WriteByte((byte)remotePlayers.Count);
            foreach (var remotePlayer in remotePlayers)
            {
                Write(remotePlayer);
            }
        }

        public void WriteBool(bool value)
        {
            _netOutgoingMessage.Write(value);
        }

        public void WriteInt32(int value)
        {
            _netOutgoingMessage.Write(value);
        }

        public void WriteUInt32(uint value)
        {
            _netOutgoingMessage.Write(value);
        }

        public void WriteInt64(long value)
        {
            _netOutgoingMessage.Write(value);
        }

        public void WriteFloat(float value)
        {
            _netOutgoingMessage.Write(value);
        }

        public void WriteDouble(double value)
        {
            _netOutgoingMessage.Write(value);
        }

        public void WriteEntityId(int entityId)
        {
            WriteInt32(entityId);
        }

        public void WriteBytes(byte[] value, int size)
        {
            WriteInt16((short)size);
            _netOutgoingMessage.Write(value, 0, size);
        }

        public void WriteInt16(short value)
        {
            _netOutgoingMessage.Write(value);
        }

        public void WriteUInt16(ushort value)
        {
            _netOutgoingMessage.Write(value);
        }

        public void WriteByte(byte value)
        {
            _netOutgoingMessage.Write(value);
        }

        public void WriteBytes(byte[] value)
        {
            WriteInt16((short)value.Length);
            _netOutgoingMessage.Write(value);
        }

        public void Write(String value)
        {
            _netOutgoingMessage.Write(value);
        }

        public void Write(Vector3 value)
        {
            _netOutgoingMessage.Write(value.X);
            _netOutgoingMessage.Write(value.Y);
            _netOutgoingMessage.Write(value.Z);
        }

        public void WriteRotation(float rotation)
        {
            _netOutgoingMessage.WriteRangedSingle(rotation, -12, 12, 10);
        }

        public void WriteEntityId(Entity entity)
        {
            _netOutgoingMessage.Write(entity.EntityId);
        }

        public void Write(Entity entity)
        {
            WriteEntityId(entity);
            WriteInt16(entity.EntityTypeId);
            WriteEntityId(entity.Parent);

            Write(entity.NonDefaultProperties, (short) entity.NonDefaultPropertyCount);
        }

        public void Write(List<string> strings)
        {
            if (strings.Count > 255)
            {
                throw new ApplicationException(
                    "More than 255 strings, you kiddin me bro!?");
            }

            _netOutgoingMessage.Write((byte)strings.Count);
            foreach (var s in strings)
            {
                _netOutgoingMessage.Write(s);
            }
        }

        public void Write(Color4 colour)
        {
            WriteByte((byte)(colour.Alpha * 255));
            WriteByte((byte)(colour.Red * 255));
            WriteByte((byte)(colour.Green * 255));
            WriteByte((byte)(colour.Blue * 255));
        }

        public void Write(List<ILight> lights)
        {
            WriteInt16((short)lights.Count);

            foreach (var light in lights)
            {
                Write(light);
            }
        }

        public void Write(ILight light)
        {
            Write(light.Position);
            WriteFloat(light.Brightness);
            Write(light.Colour);
            WriteBool(light.IsDynamic);
            WriteBool(light.Enabled);
        }

        public void Write<T>(T property) where T : Trait
        {
            WriteInt16(property.PropertyId);
            WriteBytes(property.ByteArrayValue);
        }

        public void Write(List<Entity> entities)
        {
            _netOutgoingMessage.EnsureBufferSize(30 * entities.Count * 8 + _netOutgoingMessage.LengthBits);
            WriteInt16((short)entities.Count);
            foreach (var entity in entities)
            {
                Write(entity);
            }
        }

        public void Write(List<short> shorts)
        {
            WriteInt16((short)shorts.Count);
            foreach (var s in shorts)
            {
                WriteInt16(s);
            }
        }

        public void Write(List<int> ints)
        {
            WriteInt16((short)ints.Count);
            foreach (var s in ints)
            {
                WriteInt32(s);
            }
        }

        public void Write(ChunkMesh chunkMesh)
        {
            // write out vectors
            WriteInt32(chunkMesh.Triangles.Count);
            foreach (var triangle in chunkMesh.Triangles)
            {
                Debug.Assert(triangle.Material < 255);
                WriteByte((byte)triangle.Material);
                WriteUInt16((ushort) triangle.Vertex0);
                WriteUInt16((ushort) triangle.Vertex1);
                WriteUInt16((ushort) triangle.Vertex2);
            }

            // write out triangles
            WriteInt32(chunkMesh.Vertices.Count);
            foreach (var vector in chunkMesh.Vertices)
            {
                WriteFloat(vector.X);
                WriteFloat(vector.Y);
                WriteFloat(vector.Z);
            }
        }

        public void Write(List<Chunk> chunks)
        {
            WriteByte((byte)chunks.Count);
            foreach (var chunk in chunks)
            {
                Write(chunk);
            }
        }

        public void Write(Chunk chunk)
        {
            Write(chunk.Key);
            Write(chunk.ChunkMesh);
            Write(chunk.Lights);
        }

        public void Write(ChunkKey chunkKey)
        {
            _netOutgoingMessage.Write(chunkKey.X);
            _netOutgoingMessage.Write(chunkKey.Y);
        }

        public void Write(List<ChunkKey> chunkKeys)
        {
            WriteByte((byte)chunkKeys.Count);
            foreach (var chunkKey in chunkKeys)
            {
                Write(chunkKey);
            }
        }

        public void Write<T>(IEnumerable<T> properties, short propertyCount) where T : Trait
        {
            WriteInt16(propertyCount);
            foreach (var property in properties)
                Write(property);
        }

        public void Write<T>(List<T> properties) where T : Trait
        {
            Write(properties, (short) properties.Count);
        }
    }
}