using System.Collections.Generic;
using System.IO;
using Psy.Core.Serialization;
using SlimMath;
using Vortex.Interface.Traits;
using Vortex.Interface.World;
using Vortex.Interface.World.Chunks;

namespace Vortex.Interface.Serialisation
{
    public static class StreamExtensions
    {
        public static void Write(this Stream stream, byte[] data)
        {
            stream.Write(data.Length);
            stream.Write(data, 0, data.Length);
        }

        public static byte[] ReadByteArray(this Stream stream)
        {
            var size = stream.ReadInt();
            var buffer = new byte[size];
            stream.Read(buffer, 0, size);
            return buffer;
        }


        /**
         */
        public static void Write(this Stream stream, ChunkKey key)
        {
            stream.Write(key.X);
            stream.Write(key.Y);
        }

        public static ChunkKey ReadChunKey(this Stream stream)
        {
            var x = stream.ReadInt();
            var y = stream.ReadInt();

            return new ChunkKey(x, y);
        }

        /**
         */
        public static void Write(this Stream stream, Chunk chunk)
        {
            stream.Write(chunk.Key);
            stream.Write(chunk.ChunkMesh);
            stream.Write(chunk.Lights);
        }

        public static Chunk ReadChunk(this Stream stream)
        {
            var key = stream.ReadChunKey();
            var mesh = stream.ReadChunkMesh();
            var lights = stream.ReadLights();

            return new Chunk(key, mesh, lights);
        }

        /**
         */
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

        public static void Write(this Stream stream, ILight light)
        {
            stream.Write(light.IsDynamic);
            stream.Write(light.Enabled);
            stream.Write(light.Brightness);
            stream.Write(light.Position);
            stream.Write(light.Colour);
        }

        public static List<ILight> ReadLights(this Stream stream)
        {
            var numLights = stream.ReadInt();
            var lights = new List<ILight>(numLights);

            for (var i = 0; i < numLights; i++)
            {
                lights.Add(ReadLight(stream));
            }
            return lights;
        }

        public static void Write(this Stream stream, List<ILight> lights)
        {
            stream.Write(lights.Count);

            foreach (var light in lights)
            {
                stream.Write(light);
            }
        }

        /**
         */
        public static void Write(this Stream stream, ChunkMesh chunkMesh)
        {
            stream.Write(chunkMesh.Triangles.Count);
            foreach (var triangle in chunkMesh.Triangles)
            {
                stream.Write((byte)triangle.Material);
                stream.Write((ushort)triangle.Vertex0);
                stream.Write((ushort)triangle.Vertex1);
                stream.Write((ushort)triangle.Vertex2);
            }

            // write out triangles
            stream.Write(chunkMesh.Vertices.Count);
            foreach (var vector in chunkMesh.Vertices)
            {
                stream.Write(vector.X);
                stream.Write(vector.Y);
                stream.Write(vector.Z);
            }
        }

        public static ChunkMesh ReadChunkMesh(this Stream stream)
        {
            var chunkMesh = new ChunkMesh();

            var triangleCount = stream.ReadInt();
            for (var i = 0; i < triangleCount; i++)
            {
                var material = stream.ReadByte();
                var v0 = (ushort)stream.ReadShort();
                var v1 = (ushort)stream.ReadShort();
                var v2 = (ushort)stream.ReadShort();

                var triangle = new ChunkMeshTriangle(material, v0, v1, v2, chunkMesh);
                chunkMesh.Triangles.Add(triangle);
            }

            var vertexCount = stream.ReadInt();
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

        /**
         */
        
        public static void Write<TTrait>(this Stream stream, List<TTrait> traits)
            where TTrait : Trait
        {
            stream.Write(traits, traits.Count);
        }

        public static void Write<TTrait>(this Stream stream, IEnumerable<TTrait> traits, int count)
            where TTrait : Trait
        {
            stream.Write(count);
            foreach (var trait in traits)
                stream.Write(trait);
        }

        public static List<TTrait> ReadTraits<TTrait>(this Stream stream)
            where TTrait : Trait, new()
        {
            var count = stream.ReadInt();
            var traits = new List<TTrait>(count);
            for(var i=0; i<count; ++i)
            {
                var trait = stream.ReadTrait<TTrait>();
                traits.Add(trait);
            }
            return traits;
        }

        public static void Write(this Stream stream, Trait trait)
        {
            stream.Write(trait.PropertyId);
            stream.Write(trait.Value);
        }

        public static TTrait ReadTrait<TTrait>(this Stream stream)
            where TTrait: Trait, new()
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