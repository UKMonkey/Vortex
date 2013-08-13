using SlimMath;
using Vortex.Interface.EntityBase;
using Vortex.World.Interfaces;

namespace Vortex.Server
{
    internal class EntityTrackingCamera : ICamera
    {
        private readonly Entity _entity;

        public Vector3 Vector
        {
            get { return _entity.GetPosition(); }
        }

        public EntityTrackingCamera(Entity entityToFollow)
        {
            _entity = entityToFollow;
        }
    }
}
