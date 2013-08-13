using Psy.Core.Collision;
using SlimMath;

namespace Vortex.World.Observable
{
    public class ObservableAreaCollisionTester : MeshCollisionTester
    {
        private readonly Vector3 _bottomLeft;

        public ObservableAreaCollisionTester(Mesh mesh, Vector3 bottomLeft)
            :base(mesh)
        {
            _bottomLeft = bottomLeft;
        }


        public override MeshCollisionResult CollideWithRay(Vector3 point, Vector3 direction)
        {
            var newPoint = point - _bottomLeft;
            var result = base.CollideWithRay(newPoint, direction);

            if (result.RayCollisionResult.HasCollided)
            {
                result.RayCollisionResult.CollisionPoint += _bottomLeft;
            }

            return result;
        }
    }
}
