using Psy.Graphics;
using SlimMath;

namespace Vortex.Renderer.Camera
{
    public class ManualCamera : BasicCamera
    {
        public Vector3 ManualVector;

        public ManualCamera(GraphicsContext graphicsContext, Vector3 position)
            : base(graphicsContext)
        {
            ManualVector = position;
        }

        protected override Vector3 GetPosition()
        {
            return ManualVector;
        }
    }
}
