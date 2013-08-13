using System;
using Psy.Core;
using Psy.Graphics;
using Psy.Graphics.VertexDeclarations;
using SlimMath;
using Vortex.BulletTracer;

namespace Vortex.Renderer.BulletTracer
{
    public class BulletRenderer : IDisposable
    {
        //private const float BulletZ = -0.5f;
        private const int MaxVertexCount = 200 * 6;
        private readonly GraphicsContext _graphicsContext;
        private readonly IVertexRenderer<ColouredVertex4> _vertexRenderer;

        public BulletRenderer(GraphicsContext graphicsContext)
        {
            _graphicsContext = graphicsContext;
            _vertexRenderer = graphicsContext.CreateVertexRenderer<ColouredVertex4>(MaxVertexCount);
        }

        public void Dispose()
        {
            if (_vertexRenderer != null) _vertexRenderer.Dispose();
        }

        public void Render(BulletCollection bullets)
        {
            if (bullets.Count == 0)
                return;

            _graphicsContext.World = Matrix.Identity;

            var vertexCount = 0;
            var vertexStream = _vertexRenderer.LockVertexBuffer();

            foreach (var bullet in bullets)
            {
                // we can only draw a maximum number of bullets, the rest
                // we'll just have to ignore for now.
                if (vertexCount == MaxVertexCount)
                    break;

                var angle = bullet.MovementVector.ZPlaneAngle() - (Math.PI/2);
                var x = (float) Math.Cos(angle);
                var y = -(float) Math.Sin(angle + Math.PI);
                var vec = new Vector3(x * 0.015f, y * 0.015f, 0);

                // blue - top (current)
                var currentBlue = new ColouredVertex4
                                 {
                                     Colour = new Color4(0.8f, 0.3f, 0.3f, 0.3f),
                                     Position = new Vector3
                                                    {
                                                        X = vec.X + bullet.CurrentPosition.X,
                                                        Y = vec.Y + bullet.CurrentPosition.Y,
                                                        Z = bullet.CurrentPosition.Z
                                                    }
                                 };
                

                // red - bottom (current)
                var currentRed = new ColouredVertex4
                                 {
                                     Colour = new Color4(0.8f, 0.7f, 0.7f, 0.7f),
                                     Position = new Vector3
                                                    {
                                                        X = -vec.X + bullet.CurrentPosition.X,
                                                        Y = -vec.Y + bullet.CurrentPosition.Y,
                                                        Z = bullet.CurrentPosition.Z
                                                    }
                                 };

                // -----------------------------------------------------------
                // blue - top (start)
                var startBlue = new ColouredVertex4
                                          {
                                              Colour = new Color4(0.0f, 0.0f, 0.0f, 0.0f),
                                              Position = new Vector3
                                                             {
                                                                 X = vec.X + bullet.StartPosition.X,
                                                                 Y = vec.Y + bullet.StartPosition.Y,
                                                                 Z = bullet.CurrentPosition.Z
                                                             }
                                          };
                
                // red - top (start)
                var startRed = new ColouredVertex4
                                          {
                                              Colour = new Color4(0.0f, 0.0f, 0.0f, 0.0f),
                                              Position = new Vector3
                                                             {
                                                                 X = -vec.X + bullet.StartPosition.X,
                                                                 Y = -vec.Y + bullet.StartPosition.Y,
                                                                 Z = bullet.CurrentPosition.Z
                                                             }
                                          };

                vertexStream.Write(startBlue);
                vertexStream.Write(startRed);
                vertexStream.Write(currentBlue);

                vertexStream.Write(currentBlue);
                vertexStream.Write(startRed);
                vertexStream.Write(currentRed);

                vertexCount += 6;
            }

            _vertexRenderer.UnlockVertexBuffer();

            _vertexRenderer.Render(PrimitiveType.TriangleList, 0, vertexCount / 3);
        }
    }
}
