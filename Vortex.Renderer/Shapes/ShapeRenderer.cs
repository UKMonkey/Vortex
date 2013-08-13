using System;
using System.Collections.Generic;
using Psy.Graphics;
using Psy.Graphics.VertexDeclarations;

namespace Vortex.Renderer.Shapes
{
    /// <summary>
    /// Renderer of solid coloured geometry
    /// </summary>
    public class ShapeRenderer
    {
        private VerticesBuilder _verticesBuilder;
        private readonly IVertexRenderer<ColouredVertex4> _vertexRenderer;
        private readonly int _vertexCount;

        public ShapeRenderer(GraphicsContext graphicsContext)
        {
            _vertexCount = 4096;
            _vertexRenderer = graphicsContext.CreateVertexRenderer<ColouredVertex4>(_vertexCount);
        }

        public VerticesBuilder Begin()
        {
            _verticesBuilder = new VerticesBuilder(this);
            return _verticesBuilder;
        }

        internal void Render(List<ShapeVertex> vertices)
        {
            var writer = _vertexRenderer.LockVertexBuffer();

            var vertexCount = Math.Min(_vertexCount, vertices.Count);

            for (var i = 0; i < vertexCount; i++)
            {
                var cv4 = new ColouredVertex4
                              {
                                  Colour = vertices[i].Colour, 
                                  Position = vertices[i].Position
                              };
                writer.Write(cv4);
            }

            _vertexRenderer.UnlockVertexBuffer();

            _vertexRenderer.Render(PrimitiveType.TriangleList, 0, vertexCount / 3);
        }
    }
}