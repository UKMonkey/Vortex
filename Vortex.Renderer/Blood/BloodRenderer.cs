using System;
using System.Collections.Generic;
using Psy.Core;
using Psy.Graphics;
using Psy.Graphics.VertexDeclarations;
using SlimMath;
using PrimitiveType = Psy.Graphics.PrimitiveType;

namespace Vortex.Renderer.Blood
{
    public class BloodRenderer : IDisposable
    {
        public const int BloodParticleCount = 300;
        public const int SprayPerShot = 20;

        private readonly GraphicsContext _graphicsContext;
        private readonly IVertexRenderer<TexturedColouredVertex4> _vertexRenderer;
        private readonly BloodParticle[] _bloodParticles;
        private Color4 _bloodColour;
        private readonly IList<TextureAreaHolder> _textures;

        public BloodRenderer(GraphicsContext graphicsContext)
        {
            _graphicsContext = graphicsContext;
            _vertexRenderer = graphicsContext.CreateVertexRenderer<TexturedColouredVertex4>(BloodParticleCount*3);

            _bloodParticles = new BloodParticle[BloodParticleCount];
            _textures = graphicsContext.LoadTextureAtlas("blood.adf");// StaticTextureCache.TextureCache.LoadAtlas("blood.adf");
            for (var i = 0; i < _bloodParticles.Length; i++)
            {
                _bloodParticles[i] = new BloodParticle(i);
            }
        }

        public void Dispose()
        {
            if (_vertexRenderer != null) _vertexRenderer.Dispose();
        }

        public void AddShot(Vector3 position, float direction)
        {
            Reset(position, direction, SprayPerShot);
        }

        private void Reset(Vector3 position, float direction, int count = 1)
        {
            var total = count;

            foreach (var bloodParticle in _bloodParticles)
            {
                if (bloodParticle.IsDead())
                {
                    bloodParticle.Reset(position, direction);
                    total--;
                }

                if (total == 0)
                    return;
            }
        }

        public void Render()
        {
            var particleCount = 0;
            var stream = _vertexRenderer.LockVertexBuffer();

            foreach (var bloodParticle in _bloodParticles)
            {
                var texture = _textures[bloodParticle.ParticleId % _textures.Count];

                if (bloodParticle.IsDead())
                    continue;

                bloodParticle.Update();

                // tends towards 0 as particle ages
                var ageFactor = bloodParticle.Life / (float)bloodParticle.MaxLife;

                if (bloodParticle.Life > bloodParticle.MaxLife / 3.0f)
                {
                    ageFactor = 1.0f;
                }

                var angle = bloodParticle.Direction.ZPlaneAngle() - (Math.PI / 2);
                var x = (float)Math.Cos(angle);
                var y = -(float)Math.Sin(angle + Math.PI);

                var wideningAmount = 0.08f * (1.0f - ageFactor);

                var vec = new Vector3((x * 0.09f) + wideningAmount, (y * 0.09f) + wideningAmount, 0);

                _bloodColour = new Color4(ageFactor, ageFactor / 10.0f, 0.0f, 0.0f);

                stream.Write(new TexturedColouredVertex4
                {
                    Colour = _bloodColour,
                    TextureCoordinate = texture.TextureArea.AtlasBottomRight,
                    Position = bloodParticle.StartPosition.AsVector4()
                });

                stream.Write(new TexturedColouredVertex4
                {
                    Colour = _bloodColour,
                    TextureCoordinate = texture.TextureArea.AtlasTopLeft,
                    Position = bloodParticle.Position.AsVector4()
                });

                stream.Write(new TexturedColouredVertex4
                {
                    Colour = _bloodColour,
                    TextureCoordinate = new Vector2(texture.TextureArea.AtlasTopLeft.X, texture.TextureArea.AtlasBottomRight.Y),
                    Position = (bloodParticle.Position + vec).AsVector4()
                });
                
                particleCount++;
            }

            _vertexRenderer.UnlockVertexBuffer();
            var cullMode = _graphicsContext.CullMode;
            _vertexRenderer.Render(PrimitiveType.TriangleList, 0, particleCount, _textures[0].TextureArea, 0.5f, 1);
            _graphicsContext.CullMode = cullMode;
        }
    }
}