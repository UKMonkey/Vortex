using Psy.Core.Logging;
using Psy.Graphics;
using SlimMath;
using Vortex.World.Interfaces;

namespace Vortex.Renderer.Camera
{
    public abstract class BasicCamera : ICamera
    {
        private const float VerticalRotation = 0.4f;
        private readonly GraphicsContext _graphicsContext;

        public float ZoomDistance { get; set; }

        public Vector3 Vector
        {
            get { return GetPosition(); }
        }

        protected abstract Vector3 GetPosition();
        public virtual void Update() { }

        internal BasicCamera(GraphicsContext graphicsContext)
        {
            ZoomDistance = 30;
            _graphicsContext = graphicsContext;
        }

        /// <summary>
        /// Convert a screen coordinate to a view coordinate. Screen coordinates are
        /// translated from the plane (0, 0, View.FloorZ)
        /// </summary>
        /// <param name="graphicsContext"></param>
        /// <param name="x">X coordinate (from left)</param>
        /// <param name="y">Y coordinate (from bottom)</param>
        /// <returns></returns>
        public static Vector3 ScreenToWorldCoordinate(GraphicsContext graphicsContext, int x, int y)
        {
            var ray = ScreenToWorldRay(graphicsContext, x, y);
            float distance;

            var plane = new Plane(new Vector3(0, 0, -0.5f), new Vector3(0, 0, -1));
            ray.Intersects(ref plane, out distance);

            var pos = (ray.Position + (ray.Direction * distance));
            return new Vector3(pos.X, pos.Y, 0);
        }

        public static Ray ScreenToWorldRay(GraphicsContext graphicsContext, int sx, int sy)
        {
            var projectionMatrix = graphicsContext.Projection;
            var w = graphicsContext.WindowSize.Width;
            var h = graphicsContext.WindowSize.Height;
            var v = new Vector3
                        {
                            X = (((2.0f*sx)/w) - 1)/projectionMatrix.M11,
                            Y = -(((2.0f*sy)/h) - 1)/projectionMatrix.M22,
                            Z = 1.0f
                        };

            var m = graphicsContext.View;
            m.Invert();

            var rayDir = new Vector3
                             {
                                 X = v.X*m.M11 + v.Y*m.M21 + v.Z*m.M31,
                                 Y = v.X*m.M12 + v.Y*m.M22 + v.Z*m.M32,
                                 Z = v.X*m.M13 + v.Y*m.M23 + v.Z*m.M33
                             };

            // World coordinate at middle of screen
            var rayOrigin = new Vector3 {X = m.M41, Y = m.M42, Z = m.M43};

            var worldMatrix = graphicsContext.World;
            worldMatrix.Invert();

            Vector3 rayObjOrigin;
            Vector3 rayObjDirection;

            Vector3.TransformCoordinate(ref rayOrigin, ref worldMatrix, out rayObjOrigin);
            Vector3.TransformNormal(ref rayDir, ref worldMatrix, out rayObjDirection);
            rayObjDirection.Normalize();

            return new Ray(rayObjOrigin, rayObjDirection);
        }

        public Vector2 WorldToScreenCoordinate(GraphicsContext graphicsContext, Vector3 viewCoordinate)
        {
            var position = new Vector3 { X = viewCoordinate.X, Y = viewCoordinate.Y, Z = viewCoordinate.Z };

            // apply world transform
            var viewMatrix = graphicsContext.View;
            var projectionMatrix = graphicsContext.Projection;

            var position1 = Vector3.TransformCoordinate(position, viewMatrix);
            var position2 = Vector3.TransformCoordinate(position1, projectionMatrix);

            var winX = ((position2.X + 1) / 2.0f) * _graphicsContext.WindowSize.Width;
            var winY = ((1 - position2.Y) / 2.0f) * _graphicsContext.WindowSize.Height;

            return new Vector2(winX, winY);
        }

        public Matrix GetViewTransform(Vector3 offset = new Vector3())
        {
            return Matrix.Translation(-Vector.X + offset.X, -Vector.Y + offset.X, 0)
            * Matrix.RotationX(VerticalRotation)
            * Matrix.Translation(0, 0, ZoomDistance);
        }

        public Matrix GetViewTransformCameraCenteredSpace(Vector3 offset = new Vector3())
        {
            return Matrix.RotationX(VerticalRotation)
                * Matrix.Translation(0, 0, ZoomDistance);
        }
    }
}
