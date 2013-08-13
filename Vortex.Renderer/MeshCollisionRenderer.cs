using System;
using System.Collections.Generic;
using Psy.Core;
using Psy.Graphics;
using Psy.Graphics.Effects;
using Psy.Graphics.VertexDeclarations;
using SlimMath;
using Mesh = Psy.Core.Collision.Mesh;

namespace Vortex.Renderer
{
    public class MeshCollisionRenderer : IDisposable
    {
        private const int TrianglesPerBatch = 512;
        private const int BatchSize = TrianglesPerBatch * 3;

        private readonly GraphicsContext _graphicsContext;
        private readonly IVertexRenderer<ColouredVertex4> _vertexRenderer;
        private readonly List<Color4> _colours;
        private readonly List<Direction> _directions;
        private readonly IEffect _effect;
        private readonly ColouredVertex4[] _vertices;

        public MeshCollisionRenderer(GraphicsContext graphicsContext)
        {
            _graphicsContext = graphicsContext;
            _vertexRenderer = graphicsContext.CreateVertexRenderer<ColouredVertex4>(BatchSize);
            _colours = new List<Color4>
                           {
                               new Color4(1.0f, 1.0f, 0.0f, 0.0f),   //up
                               new Color4(1.0f, 0.0f, 1.0f, 0.0f), //down

                               new Color4(1.0f, 0.3f, 0.3f, 1.0f),   //west
                               new Color4(1.0f, 1.0f, 1.0f, 1.0f),            //east
                               new Color4(1.0f, 1.0f, 1.0f, 0.0f),   //north
                               new Color4(1.0f, 0.0f, 1.0f, 1.0f),   //south
                               
                               new Color4(1.0f, 0.0f, 1.0f, 1.0f),   //NorthEast
                               new Color4(1.0f, 0.0f, 0.0f, 1.0f),   //NorthWest
                               new Color4(1.0f, 1.0f, 1.0f, 1.0f),   //SouthEast
                               new Color4(1.0f, 1.0f, 0.0f, 1.0f)    //SouthWest
                           };
            _directions = new List<Direction>
                           {
                               Direction.Up,
                               Direction.Down,

                               Direction.West,
                               Direction.East,
                               Direction.North,
                               Direction.South,

                               Direction.NorthEast,
                               Direction.NorthWest,
                               Direction.SouthEast,
                               Direction.SouthWest
                           };


            _vertices = new ColouredVertex4[BatchSize];

            _effect = graphicsContext.CreateEffect("basic.fx");
        }

        public void Render(IEnumerable<Mesh> meshes, Matrix cameraMatrix, Matrix perspectiveMatrix)
        {
            var vertexIndex = 0;

            foreach (var mesh in meshes)
            {
                int triCount;
                foreach (var meshTriangle in mesh.GetAllTriangles(out triCount))
                {
                    var colour = _colours[_directions.IndexOf(meshTriangle.Direction)];

                    _vertices[vertexIndex].Colour = colour;
                    _vertices[vertexIndex].Position = mesh.Translation + meshTriangle.P0;

                    _vertices[vertexIndex+1].Colour = colour;
                    _vertices[vertexIndex+1].Position = mesh.Translation + meshTriangle.P1;

                    _vertices[vertexIndex+2].Colour = colour;
                    _vertices[vertexIndex+2].Position = mesh.Translation + meshTriangle.P2;

                    vertexIndex += 3;

                    if (vertexIndex >= BatchSize-2)
                    {
                        RenderBatch(vertexIndex /3, cameraMatrix, perspectiveMatrix);
                        vertexIndex = 0;
                    }
                }
            }

            if (vertexIndex != 0)
            {
                RenderBatch(vertexIndex / 3, cameraMatrix, perspectiveMatrix);
            }
        }

        private void RenderBatch(int primitiveCount, Matrix cameraMatrix, Matrix perspectiveMatrix)
        {
            var prevCullMode = _graphicsContext.CullMode;
            var zFunc = _graphicsContext.ZCompareFunctionFunction;

            _graphicsContext.FillMode = FillMode.Wireframe;
            _graphicsContext.CullMode = CullMode.CCW;
            _graphicsContext.ZCompareFunctionFunction = ZCompareFunction.Always;

            var vertexStream = _vertexRenderer.LockVertexBuffer();

            vertexStream.WriteRange(_vertices);

            _vertexRenderer.UnlockVertexBuffer();

            var matrix = cameraMatrix * perspectiveMatrix;
            _effect.SetMatrix("worldViewProjMat", matrix);

            _effect.Begin();
            _effect.BeginPass(0);
            _vertexRenderer.Render(PrimitiveType.TriangleList, 0, primitiveCount);
            _effect.EndPass();
            _effect.End();

            _graphicsContext.FillMode = FillMode.Solid;
            _graphicsContext.CullMode = prevCullMode;
            _graphicsContext.ZCompareFunctionFunction = zFunc;
        }

        public void Dispose()
        {
            if (_vertexRenderer != null) _vertexRenderer.Dispose();
        }
    }
}