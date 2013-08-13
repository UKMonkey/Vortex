using System;
using Psy.Core.Console;
using Psy.Graphics;
using SlimMath;
using Vortex.PerformanceHud;
using Vortex.Renderer.PerformanceHud;

namespace Vortex.Client.Renderer
{
    public class PerformanceChartRenderer : IDisposable
    {
        private readonly TimeLineRenderer _timeLineRenderer;
        private readonly TimeLineRenderer _memoryTimeLineRenderer;
        private readonly TimeLineRenderer _inboundKbpsTimeLineRenderer;
        private readonly TimeLineRenderer _outboundKbpsTimeLineRenderer;
        private readonly BarChartRenderer _netPerformanceBarChartRenderer;
        private bool Visible { get; set; }


        public PerformanceChartRenderer(GraphicsContext graphicsContext, 
            TimeLine updateLoopTimeLine, TimeLine memoryTimeLine, TimeLine inboundKbpsTimeLine, 
            TimeLine outboundKbpsTimeLine, BarChart netPerformanceBarChart)
        {
            Visible = false;
            var topRight = new Vector2(20, 20);

            StaticConsole.Console.RegisterFloat(
                "perfInfo", () => Visible ? 1 : 0, 
                delegate(float f) { Visible = f > 0; });

            _timeLineRenderer = new TimeLineRenderer(graphicsContext, updateLoopTimeLine)
            {
                MainColor = new Color4(0.8f, 1.0f, 1.0f, 0.0f),
                DimColor = new Color4(0.2f, 1.0f, 0.8f, 0.0f),
                TextLineOffset = 0,
                Height = 60,
                TopRight = topRight
            };

            _memoryTimeLineRenderer = new TimeLineRenderer(graphicsContext, memoryTimeLine)
            {
                MainColor = new Color4(0.7f, 1.0f, 0.0f, 1.0f),
                DimColor = new Color4(0.01f, 0.2f, 0.0f, 0.2f),
                TextLineOffset = 1,
                Height = 60,
                TopRight = topRight
            };

            _inboundKbpsTimeLineRenderer = new TimeLineRenderer(graphicsContext, inboundKbpsTimeLine)
            {
                MainColor = new Color4(0.7f, 1.0f, 0.0f, 0.0f),
                TextLineOffset = 2,
                Height = 60,
                TopRight = topRight
            };

            _outboundKbpsTimeLineRenderer = new TimeLineRenderer(graphicsContext, outboundKbpsTimeLine)
            {
                MainColor = new Color4(0.7f, 0.0f, 1.0f, 0.0f),
                TextLineOffset = 3,
                Height = 60,
                TopRight = topRight
            };

            _netPerformanceBarChartRenderer = new BarChartRenderer(graphicsContext, netPerformanceBarChart);
        }

        public void Render()
        {
            if (!Visible)
                return;

            _timeLineRenderer.Render();
            _memoryTimeLineRenderer.Render();
            _inboundKbpsTimeLineRenderer.Render();
            _outboundKbpsTimeLineRenderer.Render();
            _netPerformanceBarChartRenderer.Render();
        }

        public void Dispose()
        {
            _timeLineRenderer.Dispose();
            _memoryTimeLineRenderer.Dispose();
            _inboundKbpsTimeLineRenderer.Dispose();
            _outboundKbpsTimeLineRenderer.Dispose();
            _netPerformanceBarChartRenderer.Dispose();
        }
    }
}