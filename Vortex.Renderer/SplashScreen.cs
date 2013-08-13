using System;
using Psy.Core;
using Psy.Graphics;
using Psy.Graphics.VertexDeclarations;

namespace Vortex.Renderer
{
    enum VisibilityState
    {
        Showing,
        Hiding,
        Visible,
        Hidden
    }

    public class SplashScreen : IDisposable
    {
        private TextureAreaHolder _textureArea;
        private readonly GraphicsContext _graphicsContext;
        private VisibilityState _visibilityState;
        private float _alpha;
        public bool Visible
        {
            get 
            { 
                return (
                    _visibilityState == VisibilityState.Showing 
                    || _visibilityState == VisibilityState.Visible); 
            }
            set
            {
                if (value)
                {
                    if (_visibilityState != VisibilityState.Visible)
                    {
                        _visibilityState = VisibilityState.Showing;
                    }
                }
                else
                {
                    if (_visibilityState != VisibilityState.Hidden)
                    {
                        _visibilityState = VisibilityState.Hiding;
                    }
                }
            }
        }
        private string ImageFileName { get; set; }
        private readonly IVertexRenderer<TransformedColouredTexturedVertex> _vertexRenderer;

        private const int VertexCount = 6;
        private const float FrameAlphaAdjustAmount = 0.04f;

        public SplashScreen(GraphicsContext graphicsContext)
        {
            _graphicsContext = graphicsContext;
            _visibilityState = VisibilityState.Hidden;
            _alpha = 0.0f;
            ImageFileName = null;

            _vertexRenderer = graphicsContext.CreateVertexRenderer<TransformedColouredTexturedVertex>(VertexCount);
        }

        public void Dispose()
        {
            if (_vertexRenderer != null) _vertexRenderer.Dispose();
        }

        private void WriteVertices(IDataStream<TransformedColouredTexturedVertex> vertexStream)
        {
            float minX = 1.0f;
            float minY = 1.0f;

            const float z = 1.0f;

            var color = new SlimMath.Color4(_alpha, 1.0f, 1.0f, 1.0f);

            float maxY = _graphicsContext.WindowSize.Height;
            float maxX = _graphicsContext.WindowSize.Width;
            var area = _textureArea.TextureArea;

            float aspectRatio = area.Width/(float)area.Height;
            float splashImageWidth = (maxY*aspectRatio);

            minX = (_graphicsContext.WindowSize.Width - splashImageWidth) / 2.0f;
            maxX = minX + splashImageWidth;

            vertexStream.WriteRange(new[]
            {
                // |/
                new TransformedColouredTexturedVertex(
                    new SlimMath.Vector4(minX, minY, z, 1.0f), 
                    new SlimMath.Vector2(0.0f, 0.0f), color),
                new TransformedColouredTexturedVertex(
                    new SlimMath.Vector4(maxX, minY, z, 1.0f), 
                    new SlimMath.Vector2(1.0f, 0.0f), color),
                new TransformedColouredTexturedVertex(
                    new SlimMath.Vector4(maxX, maxY, z, 1.0f), 
                    new SlimMath.Vector2(1.0f, 1.0f), color),
                
                // /|
                new TransformedColouredTexturedVertex(
                    new SlimMath.Vector4(minX, minY, z, 1.0f), 
                    new SlimMath.Vector2(0.0f, 0.0f), color),
                new TransformedColouredTexturedVertex(
                    new SlimMath.Vector4(maxX, maxY, z, 1.0f), 
                    new SlimMath.Vector2(1.0f, 1.0f), color),
                new TransformedColouredTexturedVertex(
                    new SlimMath.Vector4(minX, maxY, z, 1.0f), 
                    new SlimMath.Vector2(0.0f, 1.0f), color)
            });
        }

        private void UpdateVertexBuffer(TextureAreaHolder tah)
        {
            PopulateVertexBuffer();
        }

        private void LoadImage()
        {
            _textureArea = _graphicsContext.GetTexture(ImageFileName);
            _textureArea.OnChange += UpdateVertexBuffer;
            PopulateVertexBuffer();
        }

        private void PopulateVertexBuffer()
        {
            var vertexStream = _vertexRenderer.LockVertexBuffer();
            WriteVertices(vertexStream);
            _vertexRenderer.UnlockVertexBuffer();
        }

        public void SetImage(string imageFileName)
        {
            ImageFileName = imageFileName;
            LoadImage();
        }

        public void Render()
        {
            if (_visibilityState == VisibilityState.Hidden)
                return;

            var magFilter = _graphicsContext.MagFilter;
            var minFilter = _graphicsContext.MinFilter;

            _graphicsContext.ZBufferEnabled = false;
            _graphicsContext.ZWriteEnabled = false;

            _vertexRenderer.Render(PrimitiveType.TriangleList, 0, 2, _textureArea.TextureArea, 0, 1);

            _graphicsContext.ZBufferEnabled = true;
            _graphicsContext.ZWriteEnabled = true;

            _graphicsContext.MinFilter = minFilter;
            _graphicsContext.MagFilter = magFilter;

        }

        public void Update()
        {
            switch (_visibilityState)
            {
                case VisibilityState.Showing:
                    _alpha += FrameAlphaAdjustAmount;
                    if (_alpha >= 1.0f)
                    {
                        _visibilityState = VisibilityState.Visible;
                        _alpha = 1.0f;
                    }
                    break;
                case VisibilityState.Hiding:
                    _alpha -= FrameAlphaAdjustAmount;
                    if (_alpha <= 0.0f)
                    {
                        _visibilityState = VisibilityState.Hidden;
                        _alpha = 0.0f;
                    }
                    break;
            }
            PopulateVertexBuffer();
        }
    }
}
