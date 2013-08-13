using System;
using Psy.Core.Console;
using Psy.Graphics;
using SlimMath;
using Vortex.PerformanceHud;
using Vortex.Renderer.PerformanceHud;

namespace Vortex.Client.Renderer
{
    public class LatencyRenderer : IDisposable
    {
        private bool Visible { get; set; }
        private readonly TimeLineRenderer _timeLineRenderer;
        private Vector2 _topRight = new Vector2(20, 190);

        public LatencyRenderer(GraphicsContext gc, TimeLine latencyTimeLine)
        {
            Visible = false;
            _timeLineRenderer = new TimeLineRenderer(gc, latencyTimeLine)
            {
                MainColor = new Color4(0.8f, 1.0f, 1.0f, 0.0f),
                DimColor = new Color4(0.2f, 1.0f, 0.8f, 0.0f),
                TextLineOffset = 0,
                TopRight = _topRight,
                ForcedMinValue = 0
            };

            StaticConsole.Console.RegisterFloat(
                "latencyInfo", () => Visible ? 1 : 0,
                delegate(float f) { Visible = f > 0; });
        }

        public void Render()
        {
            if (!Visible)
                return;

            _timeLineRenderer.Render();
        }

        public void Dispose()
        {
            _timeLineRenderer.Dispose();
        }
    }
}
