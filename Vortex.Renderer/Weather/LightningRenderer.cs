using System;
using Psy.Core;
using Psy.Graphics;
using Psy.Graphics.VertexDeclarations;

namespace Vortex.Renderer.Weather
{
    public class LightningRenderer : IDisposable
    {
        private const int FlashDuration = 3;
        private const int MinClapTime = 200;
        private const int MaxClapTime = 2000;

        private readonly GraphicsContext _graphicsContext;
        private int _clapTimer;
        private readonly IVertexRenderer<TransformedColouredVertex> _vertexRenderer;
        private int _timeAtWhichToClap;

        public LightningRenderer(GraphicsContext graphicsContext)
        {
            _graphicsContext = graphicsContext;
            _clapTimer = 0;
            _timeAtWhichToClap = CalculateNextClapTime();
            _vertexRenderer = graphicsContext.CreateVertexRenderer<TransformedColouredVertex>(6);
        }

        public void Dispose()
        {
            if (_vertexRenderer != null) _vertexRenderer.Dispose();
        }

        private static int CalculateNextClapTime()
        {
            return StaticRng.Random.Next(MinClapTime, MaxClapTime);
        }

        private void WriteVertices(IDataStream<TransformedColouredVertex> vertexStream)
        {
            const float minX = 1.0f;
            const float minY = 1.0f;

            const float z = 1.0f;

            var color = new SlimMath.Color4(0.1f, 0.2f, 0.2f, 0.2f);

            float maxY = _graphicsContext.WindowSize.Height;
            float maxX = _graphicsContext.WindowSize.Width;

            vertexStream.WriteRange(new[]
            {
                // |/
                new TransformedColouredVertex(
                    new SlimMath.Vector4(minX, minY, z, 1.0f), color),
                new TransformedColouredVertex(
                    new SlimMath.Vector4(maxX, minY, z, 1.0f), color),
                new TransformedColouredVertex(
                    new SlimMath.Vector4(maxX, maxY, z, 1.0f), color),
                
                // /|
                new TransformedColouredVertex(
                    new SlimMath.Vector4(minX, minY, z, 1.0f), color),
                new TransformedColouredVertex(
                    new SlimMath.Vector4(maxX, maxY, z, 1.0f), color),
                new TransformedColouredVertex(
                    new SlimMath.Vector4(minX, maxY, z, 1.0f), color)
            });
        }

        public void Render()
        {
            _clapTimer++;

            if (_clapTimer < _timeAtWhichToClap)
                return;

            if (_clapTimer > _timeAtWhichToClap + FlashDuration)
            {
                Reset();
            }

            WriteVertices(_vertexRenderer.LockVertexBuffer());
            _vertexRenderer.UnlockVertexBuffer();

            _graphicsContext.ZBufferEnabled = false;
            _vertexRenderer.Render(PrimitiveType.TriangleList, 0, 2);
            _graphicsContext.ZBufferEnabled = true;
        }

        private void Reset()
        {
            _clapTimer = 0;
            _timeAtWhichToClap = CalculateNextClapTime();
        }
    }
}