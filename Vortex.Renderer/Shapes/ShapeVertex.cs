using SlimMath;

namespace Vortex.Renderer.Shapes
{
    internal struct ShapeVertex
    {
        public Vector3 Position;
        public Color4 Colour;

        internal ShapeVertex(Vector3 position, Color4 colour)
        {
            Position = position;
            Colour = colour;
        }
    }
}