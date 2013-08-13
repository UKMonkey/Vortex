using System;
using System.Linq;
using Psy.Core;
using Psy.Graphics;
using Psy.Graphics.Text;
using Psy.Graphics.VertexDeclarations;
using SlimMath;
using Vortex.PerformanceHud;

namespace Vortex.Renderer.PerformanceHud
{
    public class TimeLineRenderer : IDisposable
    {
        private const int FontSize = 16;
        private readonly TimeLine _timeLine;
        private readonly int _vertexCount;
        private readonly IFont _font;

        public Color4 MainColor { private get; set; }
        public Color4 DimColor { private get; set; }
        public int TextLineOffset { get; set; }
        
        public Vector2 TopRight { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public double? ForcedMinValue { get; set; }

        private double _prevMaxValue;
        private int _woopwoop; // that would be the sound of the police.
        private bool _peakBlink;
        private readonly IVertexRenderer<TransformedColouredVertex> _vertexRenderer;

        public TimeLineRenderer(GraphicsContext graphicsContext, TimeLine timeLine)
        {
            _timeLine = timeLine;
            _vertexCount = timeLine.Width;
            _woopwoop = 0;

            _vertexRenderer = graphicsContext.CreateVertexRenderer<TransformedColouredVertex>(_vertexCount*2);

            MainColor = new Color4(0.9f, 0.5f, 0.5f, 1.0f).PremultiplyAlpha();
            DimColor = new Color4(0.01f, 0.2f, 0.2f, 0.4f).PremultiplyAlpha();

            _font = graphicsContext.GetFont("Consolas");
        }

        public void Dispose()
        {
            if (_vertexRenderer != null) 
                _vertexRenderer.Dispose();
        }

        public void Render()
        {
            if (_timeLine.MaxValue < _prevMaxValue)
            {
                _prevMaxValue = (_timeLine.MaxValue + _prevMaxValue) / 2;
            }
            else
            {
                _prevMaxValue = _timeLine.MaxValue;
            }

            var rollingAverage = _timeLine.RollingAverage();
            var isOverTheLimit = rollingAverage > _timeLine.AlertLevelValue;

            var mainColor = MainColor;

            if (isOverTheLimit)
            {
                _woopwoop++;
                if (_woopwoop > 30)
                {
                    _peakBlink = !_peakBlink;
                    _woopwoop = 0;
                }
                mainColor = _peakBlink ? Colours.Red : MainColor;
            }

            var vertexStream = _vertexRenderer.LockVertexBuffer();

            WriteVertices(vertexStream);
            _vertexRenderer.UnlockVertexBuffer();
            _vertexRenderer.Render(PrimitiveType.LineList, 0, _vertexCount);

            var textOffset = TextLineOffset*FontSize;
            var maxString = string.Format("{0} (Min: {1}, Max: {2}, Avg: {3}) / {4} ({5}) {6}",
                _timeLine.LastValue.ToString("0.00"),
                _timeLine.MinValue.ToString("0.00"), 
                _timeLine.MaxValue.ToString("0.00"),
                _timeLine.RollingAverage().ToString("0.00"),
                _timeLine.AlertLevelValue, 
                _timeLine.Name,
                isOverTheLimit ? "!!!!" : "");
            
            _font.DrawString(maxString, 
                (int)TopRight.X + _timeLine.Width + 10, 
                (int)TopRight.Y + 0 + textOffset, 
                mainColor);
        }

        private void WriteVertices(IDataStream<TransformedColouredVertex> vertexStream)
        {
            var mult = Height / (MaxValue() - MinValue());

            for (var i = 0; i < _timeLine.SamplePoints.Count; i++)
            {
                var value = mult * (_timeLine.SamplePoints[i] - MinValue());

                vertexStream.Write(
                    new TransformedColouredVertex(
                        new Vector4(TopRight.X + i, TopRight.Y + Height - ((float)value - 1), 0.1f, 1.0f),
                        MainColor));
                vertexStream.Write(
                    new TransformedColouredVertex(
                        new Vector4(TopRight.X + i, TopRight.Y + Height, 0.1f, 1.0f),
                        DimColor));
            }
        }

        private double MinValue()
        {
            return ForcedMinValue == null ?
                _timeLine.MinValue :
                ForcedMinValue.Value;
        }

        private double MaxValue()
        {
            return _prevMaxValue;
        }
    }
}