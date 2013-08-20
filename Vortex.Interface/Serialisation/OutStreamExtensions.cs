using System.Collections.Generic;
using System.IO;
using Psy.Core.Serialization;
using SlimMath;
using Vortex.Interface.Traits;
using Vortex.Interface.World;
using Vortex.Interface.World.Chunks;

namespace Vortex.Interface.Serialisation
{
    public static class OutStreamExtensions
    {
        public static void Write(this Stream stream, byte[] data)
        {
            stream.Write(data.Length);
            stream.Write(data, 0, data.Length);
        }

        public static void Write(this Stream stream, ChunkKey key)
        {
            stream.Write(key.X);
            stream.Write(key.Y);
        }

        public static void Write(this Stream stream, IChunk chunk, short chunkType)
        {
            stream.Write(chunkType);
            stream.Write(chunk.Key);
            stream.Write(chunk.ChunkMesh);
            stream.Write(chunk.Lights, (short) chunk.Lights.Count);
        }

        public static void Write(this Stream stream, ILight light)
        {
            stream.Write(light.IsDynamic);
            stream.Write(light.Enabled);
            stream.Write(light.Brightness);
            stream.Write(light.Position);
            stream.Write(light.Colour);
        }

        public static void Write(this Stream stream, IEnumerable<ILight> lights, short count)
        {
            stream.Write(count);

            foreach (var light in lights)
            {
                stream.Write(light);
            }
        }

        public static void Write(this Stream stream, ChunkMesh chunkMesh)
        {
            stream.Write((ushort)chunkMesh.Triangles.Count);
            foreach (var triangle in chunkMesh.Triangles)
            {
                stream.Write((byte)triangle.Material);
                stream.Write((ushort)triangle.Vertex0);
                stream.Write((ushort)triangle.Vertex1);
                stream.Write((ushort)triangle.Vertex2);
            }

            // write out triangles
            stream.Write((ushort)chunkMesh.Vertices.Count);
            foreach (var vector in chunkMesh.Vertices)
            {
                stream.Write((float)vector.X);
                stream.Write((float)vector.Y);
                stream.Write((float)vector.Z);
            }
        }
        
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

        public static void Write(this Stream stream, Trait trait)
        {
            stream.Write(trait.PropertyId);
            stream.Write(trait.Value);
        }
    }
}