using System.Collections.Generic;
using Psy.Core;
using SlimMath;

namespace Vortex.Renderer.Shapes
{
    public class VerticesBuilder
    {
        protected ShapeRenderer ShapeRenderer { get; set; }
        private List<ShapeVertex> Vertices { get; set; }

        public VerticesBuilder(ShapeRenderer shapeRenderer)
        {
            ShapeRenderer = shapeRenderer;
            Vertices = new List<ShapeVertex>();
        }

        public VerticesBuilder Plane(Vector3 bottomLeft, Vector3 topRight, Color4 colour)
        {
            var plane = new Plane(bottomLeft, topRight);
            plane.WriteVertices(v => Vertices.Add(new ShapeVertex(v, colour)));
            return this;
        }

        public VerticesBuilder SelectedCube(Vector3 position, Color4 colour, float size, float height = 0.5f)
        {
            Cube(position.Translate(-size / 2, -size / 2, 0), colour.MakeSolid(), size/8, height);
            Cube(position.Translate(-size / 2, size / 2, 0), colour.MakeSolid(), size / 8, height);
            Cube(position.Translate(size / 2, -size / 2, 0), colour.MakeSolid(), size / 8, height);
            Cube(position.Translate(size / 2, size / 2, 0), colour.MakeSolid(), size / 8, height);
            return this;
        }

        public VerticesBuilder Cube(Vector3 position, Color4 colour, float size, float height = 0.5f)
        {
            position.Z = 1.0f;

            var farBottomLeft = position.Translate(-size/2, -size/2, 0);
            var farTopRight = position.Translate(size/2, size/2, 0);
            var farTopLeft = position.Translate(-size/2, size/2, 0);
            var farBottomRight = position.Translate(size/2, -size/2, 0);
            
            var nearBottomLeft = position.Translate(-size/2, -size/2, -height);
            var nearBottomRight = position.Translate(size/2, -size/2, -height);
            var nearTopRight = position.Translate(size/2, size/2, -height);
            var nearTopLeft = position.Translate(-size/2, size/2, -height);

            var topPlane = new Plane(nearBottomLeft, nearTopRight);
            var frontPlane = new Plane(farBottomLeft, nearBottomRight);
            var leftPlane = new Plane(farTopLeft, nearBottomLeft);
            var rightPlane = new Plane(farBottomRight, nearTopRight);
            var backPlane = new Plane(farTopRight, nearTopLeft);

            topPlane.WriteVertices(v => Vertices.Add(new ShapeVertex(v, colour)));
            frontPlane.WriteVertices(v => Vertices.Add(new ShapeVertex(v, colour)));
            leftPlane.WriteVertices(v => Vertices.Add(new ShapeVertex(v, colour)));
            rightPlane.WriteVertices(v => Vertices.Add(new ShapeVertex(v, colour)));
            backPlane.WriteVertices(v => Vertices.Add(new ShapeVertex(v, colour)));

            return this;
        }

        public void Render()
        {
            ShapeRenderer.Render(Vertices);
        }
    }
}