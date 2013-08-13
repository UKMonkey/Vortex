using System;
using Psy.Graphics;
using Psy.Graphics.Text;
using Vortex.PerformanceHud;

namespace Vortex.Renderer.PerformanceHud
{
    public class BarChartRenderer : IDisposable
    {
        private const int FontSize = 13;

        private readonly GraphicsContext _graphicsContext;
        private readonly BarChart _barChart;
        private readonly IFont _font;
        private int _width;

        public BarChartRenderer(GraphicsContext graphicsContext, BarChart barChart)
        {
            _width = 0;
            _graphicsContext = graphicsContext;
            _barChart = barChart;
            _font = graphicsContext.GetFont(fontFace: "Consolas", fontSize: FontSize);

            barChart.OnChange += BarChartUpdate;
        }

        public void Dispose()
        {
            _barChart.OnChange -= BarChartUpdate;
        }

        private void BarChartUpdate(string groupName, string barname)
        {
            if (_width == 0)
            {
                _width = CalculateWidth();
                return;
            }

            var bar = _barChart.BarGroups[groupName].Bars[barname];
            var barText = GetTextForBar(barname, bar);
            var width = GetTextWidth(barText);

            if (width > _width)
                _width = width;
        }

        private static string GetTextForBar(string name, Bar bar)
        {
            return string.Format("{0} - {1}", name, bar.Value.ToString("0.00"));
        }

        private int GetTextWidth(string barText)
        {
            return _font.MeasureString(barText, TextFormat.Left).Width;
        }

        public void Render()
        {
            // todo: render these as snazzy bars.

            var x = _graphicsContext.WindowSize.Width - _width;
            var y = 120;
            foreach (var barGroup in _barChart.BarGroups)
            {
                var barGroupText = string.Format("------ {0} ------", barGroup.Key);
                _font.DrawString(barGroupText, x, y, new SlimMath.Color4(1.0f, 1.0f, 1.0f, 1.0f));
                y += FontSize;

                foreach (var bar in barGroup.Value.Bars)
                {
                    var barText = GetTextForBar(bar.Key, bar.Value);
                    _font.DrawString(barText, x, y, new SlimMath.Color4(1.0f, 1.0f, 1.0f, 1.0f));
                    y += FontSize;
                }
            }
        }

        private int CalculateWidth()
        {
            var width = 0;   

            foreach (var barGroup in _barChart.BarGroups)
            {
                foreach (var bar in barGroup.Value.Bars)
                {
                    var barText = GetTextForBar(bar.Key, bar.Value);
                    width = Math.Max(width, GetTextWidth(barText));
                }
            }

            return width;
        }
    }
}