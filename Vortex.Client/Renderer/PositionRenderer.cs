using System;
using Psy.Core.Console;
using Psy.Graphics;
using Psy.Graphics.Text;
using SlimMath;
using Vortex.Renderer;
using Vortex.World.Chunks;

namespace Vortex.Client.Renderer
{
    public class PositionRenderer : IDisposable
    {
        private bool _visible;
        private readonly View _view;
        private readonly IFont _font;
        private readonly Color4 _colour;
        private Vector2 _topRight = new Vector2(20, 250);

        public PositionRenderer(GraphicsContext graphicsContext, View view)
        {
            StaticConsole.Console.RegisterFloat(
                "posInfo", () => _visible ? 1 : 0,
                delegate(float f) { _visible = f > 0; });

            _view = view;
            _font = graphicsContext.GetFont("Consolas");
            _colour = new Color4(0.7f, 0.5f, 0.5f);
        }

        public void Render()
        {
            if (!_visible)
                return;

            Vector3? position;
            if (_view.CameraPosition == null)
                position = null;
            else
                position = _view.CameraPosition.Vector;

            var positionText = GetPositionText(position);
            var chunkText = GetChunkText(position);

            _font.DrawString(positionText, (int)_topRight.X, (int)_topRight.Y, _colour);
            _font.DrawString(chunkText, (int)_topRight.X, (int)_topRight.Y + 15, _colour);
        }

        private static string GetPositionText(Vector3? position)
        {
            if (position == null)
                return "Position:   x: -  y: -  z: -";

            return string.Format("Position:    x: {0}  y: {1}  z: {2}",
                                 Math.Round(position.Value.X),
                                 Math.Round(position.Value.Y),
                                 Math.Round(position.Value.Z));
        }

        private static string GetChunkText(Vector3? position)
        {
            if (position == null)
                return "Chunk:   x: -  y: -";

            var chunkKey = Utils.GetChunkKeyForPosition(position.Value);
            return string.Format("Chunk:   x: {0}  y: {1}", 
                            chunkKey.X, chunkKey.Y);
        }

        public void Dispose()
        {
            
        }
    }
}
