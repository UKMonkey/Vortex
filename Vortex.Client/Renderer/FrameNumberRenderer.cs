using System;
using Psy.Core.Console;
using Psy.Graphics;
using Psy.Graphics.Text;
using SlimMath;
using Vortex.Interface;

namespace Vortex.Client.Renderer
{
    public class FrameNumberRenderer : IDisposable
    {
        private bool _visible;
        private readonly IFont _font;
        private readonly Color4 _colour;
        private readonly IClient _client;

        private Vector2 TopRight = new Vector2(20, 300);

        public FrameNumberRenderer(GraphicsContext graphicsContext, IClient client)
        {
            StaticConsole.Console.RegisterFloat(
                "frameNo", () => _visible ? 1 : 0,
                delegate(float f) { _visible = f > 0; });

            _font = graphicsContext.GetFont("Consolas");
            _colour = new Color4(0.7f, 0.5f, 0.5f);
            _client = client;
        }

        public void Render()
        {
            if (!_visible)
                return;

            _font.DrawString(GetCurrentFrameText(), (int)TopRight.X, (int)TopRight.Y, _colour);
            _font.DrawString(GetServerFrameText(), (int)TopRight.X, (int)TopRight.Y + 15, _colour);
            _font.DrawString(GetDifferenceFrameText(), (int)TopRight.X, (int)TopRight.Y + 30, _colour);
        }

        private string GetCurrentFrameText()
        {
            return string.Format("Current frame number: {0}", _client.CurrentFrameNumber);
        }

        private string GetServerFrameText()
        {
            return string.Format("Last known server frame number: {0}", _client.LastKnownServerFrameNumber);
        }

        private string GetDifferenceFrameText()
        {
            return string.Format("Frame number Differnce: {0}", (int)(_client.LastKnownServerFrameNumber - _client.CurrentFrameNumber));
        }

        public void Dispose()
        {
        }
    }
}
