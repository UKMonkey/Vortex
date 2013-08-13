using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SlimMath;

namespace Vortex.Interface.World
{
    public class ChunkMesh
    {
        public readonly List<ChunkMeshTriangle> Triangles;
        public readonly List<Vector3> Vertices;
        public Vector3 WorldVector;

        public ChunkMesh()
        {
            Triangles = new List<ChunkMeshTriangle>();
            Vertices = new List<Vector3>();
            WorldVector = new Vector3();
        }

        public ChunkMeshTriangle AddTriangle(int material, Vector3 p0, Vector3 p1, Vector3 p2)
        {
            Vertices.Add(p0);
            Vertices.Add(p1);
            Vertices.Add(p2);

            var triangle = new ChunkMeshTriangle(material, Vertices.Count - 3, Vertices.Count - 2, Vertices.Count - 1, this);
            Triangles.Add(triangle);
            return triangle;
        }

        public ChunkMeshTriangle[] AddRectangle(int material, Vector3 bottomLeft, Vector3 topRight)
        {
            var ret = new ChunkMeshTriangle[2];
            Debug.Assert(bottomLeft.Z == topRight.Z);

            ret[0] = AddTriangle(material, 
                                 bottomLeft, 
                                 new Vector3(bottomLeft.X, topRight.Y, topRight.Z),
                                 new Vector3(topRight.X, bottomLeft.Y, topRight.Z));
            ret[1] = AddTriangle(material, 
                                 new Vector3(topRight.X, bottomLeft.Y, topRight.Z),
                                 new Vector3(bottomLeft.X, topRight.Y, topRight.Z),
                                 topRight);
            return ret;
        }

        public void MergeMesh(ChunkMesh otherMesh)
        {
            foreach (var triangle in otherMesh.Triangles)
            {
                AddTriangle(triangle.Material, triangle.P0, triangle.P1, triangle.P2);
            }

            Compress();
        }

        public void Translate(Vector3 amount)
        {
            var tmp = new List<Vector3>(Vertices.Count);

            tmp.AddRange(Vertices.Select(item => item + amount));
            Vertices.Clear();
            Vertices.AddRange(tmp);
        }

        public void Compress()
        {
            var newVertices = new List<Vector3>(Vertices.Count);
            var newTriangles = new List<ChunkMeshTriangle>(Triangles.Count);
            var oldVertexToNewVertexMap = new Dictionary<int, int>();

            for (var i=0; i<Vertices.Count; ++i)
            {
                if (oldVertexToNewVertexMap.ContainsKey(i))
                    continue;

                var vertex = Vertices[i];
                newVertices.Add(vertex);

                oldVertexToNewVertexMap.Add(i, newVertices.Count - 1);
                
                for (var j=i+1; j<Vertices.Count; ++j)
                {
                    if (Vertices[j] == vertex)
                    {
                        oldVertexToNewVertexMap.Add(j, newVertices.Count-1);
                    }
                }
            }

            newTriangles.AddRange(
                Triangles.Select(tri => 
                    new ChunkMeshTriangle(tri.Material,
                        oldVertexToNewVertexMap[tri.Vertex0],
                        oldVertexToNewVertexMap[tri.Vertex1],
                        oldVertexToNewVertexMap[tri.Vertex2],
                        this))
                );

            Triangles.Clear();
            Vertices.Clear();

            Vertices.AddRange(newVertices);
            Triangles.AddRange(newTriangles);
        }
    }
}