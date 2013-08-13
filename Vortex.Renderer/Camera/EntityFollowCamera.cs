using Psy.Graphics;
using SlimMath;
using Vortex.Interface.EntityBase;

namespace Vortex.Renderer.Camera
{
    public class EntityFollowCamera : BasicCamera
    {
        private readonly Entity _viewEntityToFollow;

        internal EntityFollowCamera(GraphicsContext graphicsContext, Entity viewEntityToFollow)
            : base(graphicsContext)
        {
            _viewEntityToFollow = viewEntityToFollow;
        }

        protected override Vector3 GetPosition()
        {
            return _viewEntityToFollow.GetPosition();
        }
    }
}
