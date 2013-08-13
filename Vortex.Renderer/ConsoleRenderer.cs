using Psy.Core.Console;
using Psy.Graphics;
using Psy.Graphics.Text;
using Psy.Graphics.VertexDeclarations;
using SlimMath;

namespace Vortex.Renderer
{
    public class ConsoleRenderer
    {
        private IFont _font;
        private readonly Color4 _inputLineColor;
        private readonly GraphicsContext _graphicsContext;
        private readonly BaseConsole _console;
        private readonly IVertexRenderer<TransformedColouredVertex> _background;

        public bool Visible { get; set; }

        private int GetMaxRenderableConsoleLines()
        {
            return (_graphicsContext.WindowSize.Height - GetFontHeight()) / GetFontHeight();
        }

        public ConsoleRenderer(GraphicsContext graphicsContext, BaseConsole console)
        {
            _graphicsContext = graphicsContext;
            _console = console;
            Visible = true;
            _inputLineColor = new Color4(1.0f, 1.0f, 1.0f, 0.8f);
            _background = graphicsContext.CreateVertexRenderer<TransformedColouredVertex>(6);
        }

        private static string GetFontFaceName()
        {
            return "Consolas";
        }

        private static int GetFontHeight()
        {
            return 20;
        }

        private void RenderBackground()
        {
            var dataStream = _background.LockVertexBuffer();

            var width = _graphicsContext.WindowSize.Width;

            var height = GetFontHeight() * (GetMaxRenderableConsoleLines() + 1);

            var alpha = 0.9f;
            var topLeftColour = new Color4(alpha, 0.0f, 0.0f, 0.0f);
            var topRightColour = new Color4(alpha, 0.1f, 0.0f, 0.0f);
            var bottomRightColour = new Color4(alpha, 0.1f, 0.0f, 0.0f);
            var bottomLeftColour = new Color4(alpha, 0.0f, 0.0f, 0.0f);

            dataStream.WriteRange(
                new[]
                {
                    new TransformedColouredVertex
                    {
                        Colour = topLeftColour, Position = new Vector4(0, 0, 0, 1)
                    },
                    new TransformedColouredVertex
                    {
                        Colour = topRightColour, Position = new Vector4(width, 0, 0, 1)
                    },
                    new TransformedColouredVertex
                    {
                        Colour = bottomRightColour, Position = new Vector4(width, height, 0, 1)
                    },

                    new TransformedColouredVertex
                    {
                        Colour = topLeftColour, Position = new Vector4(0, 0, 0, 1)
                    },
                    new TransformedColouredVertex
                    {
                        Colour = bottomRightColour, Position = new Vector4(width, height, 0, 1)
                    },
                    new TransformedColouredVertex
                    {
                        Colour = bottomLeftColour, Position = new Vector4(0, height, 0, 1)
                    },
                });

            _background.UnlockVertexBuffer();
            _background.Render(PrimitiveType.TriangleList, 0, 6);
        }

        private void RenderInputLine()
        {
            _font.DrawString(">" + _console.InputLine + "_", 0, GetFontHeight() * GetMaxRenderableConsoleLines(), _inputLineColor);
        }

        private void RenderOutputLines()
        {
            lock (_console)
            {
                var linesAvailable = GetMaxRenderableConsoleLines();
                var fontHeight = GetFontHeight();
                var lineIndex = _console.ConsoleLines.Count - 1;

                var y = fontHeight*GetMaxRenderableConsoleLines();

                while (linesAvailable > 0)
                {
                    y -= fontHeight;

                    if (lineIndex < 0)
                        break;

                    var line = _console.ConsoleLines[lineIndex];
                    _font.DrawString(line.Text, 0, y, line.GetCalculatedColor());

                    lineIndex--;
                    linesAvailable--;
                }
            }
        }

        public void Render()
        {
            if (!Visible)
            {
                return;
            }

            if (_font == null)
            {
                _font = _graphicsContext.GetFont(GetFontFaceName(), GetFontHeight());
            }

            _graphicsContext.FillMode = FillMode.Solid;

            lock (this)
            {
                RenderBackground();
                RenderOutputLines();
                RenderInputLine();
            }
        }
    }
}
