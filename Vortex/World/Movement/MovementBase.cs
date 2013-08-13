using Psy.Core;
using Psy.Core.Collision;
using SlimMath;
using Vortex.Interface.EntityBase;
using Vortex.Interface.EntityBase.Properties;

namespace Vortex.World.Movement
{
    public abstract class MovementBase : IMovementHandler
    {
        public abstract World World { protected get; set; }
        public abstract bool IsEntityMoving(Entity item);
        public abstract bool HandleEntityMovement(Entity item);

        protected class CollisionData
        {
            public bool Collided = false;

            public bool StaticToStaticCollision = false;
            public bool MobileToMobileCollision = false;
            public Vector3 InternalPosition;
            public Vector3 CollisionPoint;
            public BoundingSphere CollisionSphere;
            public Matrix OppMatrix;
        }


        /// <summary>
        /// called for when both entities are known to be static
        /// </summary>
        /// <returns></returns>
        private CollisionData GetStaticCollisionData(Entity target, Entity toTest)
        {
            var result = new CollisionData();

            var boundingBoxA = target.Model.ModelInstance.GetBoundingBox();
            var boundingBoxB = toTest.Model.ModelInstance.GetBoundingBox();

            if (boundingBoxA.Intersects(ref boundingBoxB))
            {
                result.Collided = true;
                result.StaticToStaticCollision = true;
            }

            return result;
        }

        private CollisionData GetMobileToStaticCollisionData(Entity target, Entity toTest)
        {
            var result = new CollisionData();
            Vector3 point;

            Entity staticEntity;
            Entity mobileEntity;

            if (target.GetStatic())
            {
                staticEntity = target;
                mobileEntity = toTest;
            }
            else
            {
                mobileEntity = target;
                staticEntity = toTest;
            }

            var entityRotation = staticEntity.GetRotation();

            var matrix = Matrix.RotationZ(-entityRotation);
            var pvec = mobileEntity.GetPosition() - staticEntity.GetPosition();
            var internalPosition = Vector3.Transform(pvec, matrix).AsVector3();

            var box = staticEntity.Model.ModelInstance.GetBoundingBox();
            var sphere = new BoundingSphere(internalPosition.X, internalPosition.Y, -mobileEntity.Radius, mobileEntity.Radius);

            if (sphere.Intersects(ref box, out point))
            {
                result.Collided = true;
                result.CollisionPoint = point;
                result.CollisionSphere = sphere;
                result.InternalPosition = internalPosition;
                result.OppMatrix = Matrix.RotationZ(entityRotation);
            }

            return result;
        }

        private CollisionData MobileToMobileCollisionData(Entity target, Entity toTest)
        {
            var result = new CollisionData();
            var sphereToTest = new BoundingSphere(toTest.GetPosition().X, toTest.GetPosition().Y, -toTest.Radius, toTest.Radius);
            var sphereTarget = new BoundingSphere(target.GetPosition().X, target.GetPosition().Y, -target.Radius, target.Radius);
            float dist;

            if (sphereTarget.Intersects(ref sphereToTest, out dist))
            {
                result.Collided = true;
                result.MobileToMobileCollision = true;
                result.CollisionSphere = sphereTarget;
                result.CollisionPoint = sphereToTest.Center + ((sphereTarget.Center - sphereToTest.Center) / 2);
            }

            return result;
        }

        protected CollisionData GetCollisionData(Entity target, Entity toTest)
        {
            if (target.GetStatic() && toTest.GetStatic())
                return GetStaticCollisionData(target, toTest);
            if (target.GetStatic() || toTest.GetStatic())
                return GetMobileToStaticCollisionData(target, toTest);

            return MobileToMobileCollisionData(target, toTest);
        }
    }
}
