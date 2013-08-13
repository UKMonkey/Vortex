using System;
using Vortex.Interface.EntityBase;
using Vortex.Interface.EntityBase.Properties;

namespace Vortex.World.Movement
{
    public class RotationHandler: MovementBase
    {
        public override World World { protected get; set; }


        public override bool IsEntityMoving(Entity item)
        {
            var rotationSpeed = item.GetRotationSpeed();
            return rotationSpeed != 0;
        }

        public override bool HandleEntityMovement(Entity item)
        {
            var rotationSpeed = item.GetRotationSpeed();
            var currentRotation = item.GetRotation();
            var targetRotation = item.GetRotationTarget();
            var newRotation = rotationSpeed + currentRotation;

            var stopRotation = false;

            if (rotationSpeed > 0)
            {
                while (targetRotation < currentRotation)
                    targetRotation += (float) (2*Math.PI);
            }
            else if (rotationSpeed < 0)
            {
                while (targetRotation > currentRotation)
                    targetRotation -= (float)(2 * Math.PI);
            }

            if ((rotationSpeed > 0 && newRotation > targetRotation && currentRotation < targetRotation) ||
                (rotationSpeed < 0 && newRotation < targetRotation && currentRotation > targetRotation) ||
                targetRotation == currentRotation)
            {
                newRotation = targetRotation;
                stopRotation = true;
            }

            item.SetRotation(newRotation);

            var nearbyEntities = World.GetEntitiesWithinArea(item.GetPosition(), item.Radius);
            foreach (var entity in nearbyEntities)
            {
                var collisionData = GetCollisionData(item, entity);
                if (collisionData.Collided && !collisionData.StaticToStaticCollision) // it's ok if the door goes through static entities
                {
                    item.SetRotation(currentRotation);
                    stopRotation = false;
                    break;
                }
            }

            if (stopRotation)
            {
                item.SetRotationSpeed(0);
            }

            return true;
        }
    }
}
