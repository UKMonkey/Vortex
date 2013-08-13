using System;
using SlimMath;

namespace Vortex.Renderer.Shapes
{
    struct Plane
    {
        public Vector3 BottomLeft { get; set; }
        public Vector3 TopRight { get; set; }
        public Vector3 TopLeft { get; set; }
        public Vector3 BottomRight { get; set; }

        public Plane(Vector3 bottomLeft, Vector3 topRight) : this()
        {
            BottomLeft = bottomLeft;
            TopRight = topRight;

            if (Math.Abs(BottomLeft.Z - TopRight.Z) < 0.001f)
            {
                TopLeft = new Vector3(BottomLeft.X, TopRight.Y, TopRight.Z);
                BottomRight = new Vector3(TopRight.X, BottomLeft.Y, BottomLeft.Z);
            }
            else if (Math.Abs(BottomLeft.X - TopLeft.X) > 0.001f)
            {
                TopLeft = new Vector3(BottomLeft.X, BottomLeft.Y, TopRight.Z);
                BottomRight = new Vector3(TopRight.X, TopRight.Y, BottomLeft.Z);
            }
            else
            {
                TopLeft = new Vector3(BottomLeft.X, TopRight.Y, TopRight.Z);
                BottomRight = new Vector3(TopRight.X, BottomLeft.Y, BottomLeft.Z);
            }
        }

        /// <summary>
        /// Writes the triangle vertices for this plane.
        /// </summary>
        /// <param name="addAction"></param>
        public void WriteVertices(Action<Vector3> addAction)
        {
            addAction(BottomLeft);
            addAction(TopLeft);
            addAction(TopRight);

            addAction(BottomLeft);
            addAction(TopRight);
            addAction(BottomRight);
        }
    }
}