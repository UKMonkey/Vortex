using Psy.Core;
using SlimMath;

namespace Vortex.Interface.World
{
    public class ChunkMeshTriangle
    {
        public readonly int Material;
        public readonly int Vertex0;
        public readonly int Vertex1;
        public readonly int Vertex2;
        public ChunkMesh ChunkMesh;

        public Vector3 P0 { get { return ChunkMesh.Vertices[Vertex0]; } }
        public Vector3 P1 { get { return ChunkMesh.Vertices[Vertex1]; } }
        public Vector3 P2 { get { return ChunkMesh.Vertices[Vertex2]; } }

        public ChunkMeshTriangle(int material, int vertex0, int vertex1, int vertex2, ChunkMesh chunkMesh)
        {
            Vertex0 = vertex0;
            Material = material;
            Vertex1 = vertex1;
            Vertex2 = vertex2;
            ChunkMesh = chunkMesh;
        }

        public Vector3 GetRandomPointInWorld()
        {
            var a = StaticRng.Random.NextDouble();
            var b = StaticRng.Random.NextDouble();

            if (a + b > 1)
            {
                a = 1 - a;
                b = 1 - b;
            }

            var c = 1 - b - a;

            var x = (a*P0.X) + (b*P1.X) + (c*P2.X);
            var y = (a*P0.Y) + (b*P1.Y) + (c*P2.Y);

            return new Vector3((float)x, (float)y, 0) + ChunkMesh.WorldVector;
        }
    }
}