using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Psy.Core;
using Psy.Core.Collision;
using SlimMath;
using Vortex.Interface.EntityBase;
using Vortex.Interface.EntityBase.Properties;

namespace Vortex.World.Movement
{
    public class VectorHandler : MovementBase
    {
        public override World World { protected get; set; }
        private const float MinMove = 0.0001f;


        private bool MoveEntity(Entity item, IEnumerable<Entity> nearbyEntities, Vector3 mov)
        {
            var oldPosition = item.GetPosition();
            var newPosition = oldPosition + mov;

            item.SetPosition(newPosition);

            foreach (var entity in nearbyEntities)
            {
                var collisionData = GetCollisionData(item, entity);
                if (!collisionData.Collided)
                    continue;

                if (collisionData.StaticToStaticCollision)  // what on earth are we doing here!?!
                {
                    Debug.Assert(true);
                    continue;
                }

                // assume here that the item is mobile ... and the entity we're testing is either static or mobile

                // from the origional position, move the entity so that it's my Radius + their radius away
                if (collisionData.MobileToMobileCollision)
                {
                    var distanceVector = entity.GetPosition() - item.GetPosition();
                    var distance = distanceVector.Length - item.Radius - entity.Radius;
                    mov = (mov.NormalizeRet() * distance);
                    item.SetPosition(oldPosition + mov);
                }
                else //mobile to static collision
                {
                    var n = collisionData.CollisionSphere.Center - collisionData.CollisionPoint;
                    n.Normalize();

                    var slidingPlane = new Plane(collisionData.CollisionPoint, n);
                    var dist = Collision.DistancePlanePoint(ref slidingPlane, ref collisionData.InternalPosition);
                    var v = (dist - collisionData.CollisionSphere.Radius) * n;
                    var oppV = Vector3.Transform(v, collisionData.OppMatrix).AsVector3();

                    mov -= oppV;
                    item.SetPosition(oldPosition + mov);
                }
            }

            mov = mov.Scale(1, 1, 0);
            item.SetPosition(oldPosition + mov);

            return mov.LengthSquared > 0;
        }

        public override bool IsEntityMoving(Entity item)
        {
            if (item.GetStatic())
                return false;
            return item.GetMovementVector().Length >= MinMove;
        }

        public override bool HandleEntityMovement(Entity item)
        {
            var movementVector = item.GetMovementVector();

            var movementSpeed = movementVector.Length;

            var nearbyEntities = World.GetEntitiesWithinArea(item.GetPosition(), item.Radius + movementSpeed)
                .Where(entity => entity.GetSolid() && item.EntityId != entity.EntityId)
                .Where(entity => entity.Mesh != null);

            return MoveEntity(item, nearbyEntities, movementVector);
        }
    }
}
