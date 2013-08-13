using System;
using System.Linq;
using Psy.Core;
using Psy.Graphics;
using Psy.Graphics.VertexDeclarations;
using SlimMath;
using PrimitiveType = Psy.Graphics.PrimitiveType;

namespace Vortex.Renderer
{
    public class RayTraceRenderer : IDisposable
    {
        private readonly IVertexRenderer<ColouredVertex4> _vertexRenderer;
        private readonly RenderedRay[] _rays;
        public const int RayCount = 40;

        public RayTraceRenderer(GraphicsContext graphicsContext)
        {
            _vertexRenderer = graphicsContext.CreateVertexRenderer<ColouredVertex4>(RayCount*4);
            _rays = new RenderedRay[RayCount];

            for (var index = 0; index < _rays.Length; index++)
            {
                _rays[index] = new RenderedRay();
            }
        }

        public void Dispose()
        {
            if (_vertexRenderer != null) _vertexRenderer.Dispose();
        }

        public void AddRay(Vector3 @from, Vector3 to)
        {
            AddRay(from, to, new Color4(1.0f, 1.0f, 0.0f, 0.0f));
        }

        public void AddRay(Vector3 @from, Vector3 to, Color4 colour)
        {
            var deadRay = _rays.FirstOrDefault(r => r.IsDead());
            if (deadRay != null)
            {
                deadRay.Reset(from, to, colour);
            }

            // find the oldest ray
            var lowestLifeRay = _rays.Min(r => r.Life);
            deadRay = _rays.FirstOrDefault(r => r.Life == lowestLifeRay);
            if (deadRay != null) 
                deadRay.Reset(@from, to, colour);
        }

        public void Render()
        {
            var rayCount = 0;

            var stream = _vertexRenderer.LockVertexBuffer();

            foreach (var ray in _rays.Where(r => !r.IsDead()))
            {
                var rayColour = ray.Colour;
                stream.WriteRange(new[]
                                  {
                                      new ColouredVertex4
                                      {
                                          Colour = rayColour,
                                          Position = ray.From
                                      },
                                      new ColouredVertex4
                                      {
                                          Colour = rayColour,
                                          Position = ray.To
                                      },

                                      new ColouredVertex4
                                      {
                                          Colour = rayColour,
                                          Position = ray.To
                                      },
                                      new ColouredVertex4
                                      {
                                          Colour = rayColour,
                                          Position = ray.To.Translate(0, 0, -1.0f)
                                      },
                                  });

                rayCount++;
            }

            _vertexRenderer.UnlockVertexBuffer();
            _vertexRenderer.Render(PrimitiveType.LineList, 0, rayCount * 2);
        }

        public void Update()
        {
            foreach (var renderedRay in _rays)
            {
                renderedRay.Update();
            }
        }
    }
}