using System.Collections.Generic;
using System.IO;
using Psy.Core.Serialization;
using SlimMath;
using Vortex.Interface.Traits;
using Vortex.Interface.World;
using Vortex.Interface.World.Chunks;

namespace Vortex.Interface.Serialisation
{
    public static class InStreamExtensions
    {
        public static byte[] ReadByteArray(this Stream stream)
        {
            var size = stream.ReadInt();
            var buffer = new byte[size];
            stream.Read(buffer, 0, size);
            return buffer;
        }

        public static ChunkKey ReadChunKey(this Stream stream)
        {
            var x = stream.ReadInt();
            var y = stream.ReadInt();

            return new ChunkKey(x, y);
        }

        public static IChunk ReadChunk(this Stream stream, IEngine engine)
        {
            var type = stream.ReadShort();
            var key = stream.ReadChunKey();
            var data = stream.ReadByteArray();
            var lights = stream.ReadLights();

            var chunk = engine.ChunkFactory.GetChunk(type);
            chunk.Key = key;
            chunk.ApplyFullData(data);
            chunk.Lights.AddRange(lights);

            return chunk;
        }

        public static ILight ReadLight(this Stream stream)
        {
            return new Light
                       {
                           IsDynamic = stream.ReadBool(),
                           Enabled = stream.ReadBool(),
                           Brightness = stream.ReadFloat(),
                           Position = stream.ReadVector(),
                           Colour = stream.ReadColour()
                       };
        }

        public static List<ILight> ReadLights(this Stream stream)
        {
            var numLights = stream.ReadShort();
            var lights = new List<ILight>(numLights);

            for (var i = 0; i < numLights; i++)
            {
                lights.Add(ReadLight(stream));
            }
            return lights;
        }


        public static ChunkMesh ReadChunkMesh(this Stream stream)
        {
            var chunkMesh = new ChunkMesh();

            var triangleCount = (ushort)stream.ReadShort();
            for (var i = 0; i < triangleCount; i++)
            {
                var material = stream.ReadByte();
                var v0 = (ushort)stream.ReadShort();
                var v1 = (ushort)stream.ReadShort();
                var v2 = (ushort)stream.ReadShort();

                var triangle = new ChunkMeshTriangle(material, v0, v1, v2, chunkMesh);
                chunkMesh.Triangles.Add(triangle);
            }

            var vertexCount = (ushort)stream.ReadShort();
            for (var i = 0; i < vertexCount; i++)
            {
                var x = stream.ReadFloat();
                var y = stream.ReadFloat();
                var z = stream.ReadFloat();

                var vector = new Vector3(x, y, z);
                chunkMesh.Vertices.Add(vector);
            }

            return chunkMesh;
        }

        public static List<TTrait> ReadTraits<TTrait>(this Stream stream)
            where TTrait : Trait, new()
        {
            var count = stream.ReadInt();
            var traits = new List<TTrait>(count);
            for (var i = 0; i < count; ++i)
            {
                var trait = stream.ReadTrait<TTrait>();
                traits.Add(trait);
            }
            return traits;
        }

        public static TTrait ReadTrait<TTrait>(this Stream stream)
            where TTrait : Trait, new()
        {
            var id = stream.ReadShort();
            var data = stream.ReadByteArray();

            var ret = new TTrait();
            ret.PropertyId = id;
            ret.Value = data;

            return ret;
        }
    }
}
