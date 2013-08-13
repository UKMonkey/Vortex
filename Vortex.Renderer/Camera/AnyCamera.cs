using Psy.Graphics;
using SlimMath;
using Vortex.World.Interfaces;

namespace Vortex.Renderer.Camera
{
    public class AnyCamera : BasicCamera
    {
        public ICamera InnerCamera { get; set; }

        public AnyCamera(GraphicsContext graphicsContext, ICamera innerCamera) : base(graphicsContext)
        {
            InnerCamera = innerCamera;
        }

        protected override Vector3 GetPosition()
        {
            return InnerCamera.Vector;
        }

        public override void Update()
        {
            base.Update();
            var betterCamera = InnerCamera as BasicCamera;
            if (betterCamera != null)
            {
                betterCamera.Update();
            }
        }
    }
}