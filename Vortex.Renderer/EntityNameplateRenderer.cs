using System.Collections.Generic;
using Psy.Core;
using Psy.Graphics;
using Psy.Graphics.Text;
using Vortex.Interface.EntityBase;
using Vortex.Renderer.Camera;

namespace Vortex.Renderer
{
    public class EntityNameplateRenderer
    {
        private readonly GraphicsContext _graphicsContext;
        private readonly IFont _font;

        public EntityNameplateRenderer(GraphicsContext graphicsContext)
        {
            _graphicsContext = graphicsContext;
            _font = graphicsContext.GetFont(GetFontFaceName(), GetFontHeight());
        }

        public void Render(View view, IEnumerable<Entity> entities)
        {
            foreach (var entity in entities)
            {
                if (entity.Model == null)
                    continue;

                if (entity.GetNameplate() == "")
                    continue;

                Render(view.Camera, entity);
            }
        }

        private void Render(BasicCamera camera, Entity entity)
        {
            var model = entity.Model;
            var topVertexZ = model.ModelInstance.TopVertexZ;
            var farY = model.ModelInstance.FarY;

            var text = entity.GetNameplate();
            var textColour = entity.GetNameplateColour();

            if (text == null)
                return;

            var width = _font.MeasureString(text, TextFormat.Center).Width;

            var drawCoordinate = camera.WorldToScreenCoordinate(
                _graphicsContext,
                entity.GetPosition().Translate(0, farY, topVertexZ));

            _font.DrawString(text, (int)drawCoordinate.X - (width / 2), (int)drawCoordinate.Y - GetFontHeight(), textColour);
        }

        private static string GetFontFaceName()
        {
            return "Verdana";
        }

        private static int GetFontHeight()
        {
            return 12;
        }
    }
}