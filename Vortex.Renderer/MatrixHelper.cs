using SlimMath;
using Vortex.Interface.EntityBase;
using Vortex.World.Interfaces;

namespace Vortex.Renderer
{
    public static class MatrixHelper
    {
        public static Matrix GetEntityWorldMatrix(ICamera camera, Entity entity)
        {
            var entityPosition = entity.GetPosition() - camera.Vector;

            Matrix rotationMatrix;
            Matrix translationMatrix;

            Matrix.Translation(entityPosition.X, entityPosition.Y, 0.0f, out translationMatrix);
            Matrix.RotationZ(entity.GetRotation(), out rotationMatrix);

            return rotationMatrix * translationMatrix;
        } 
    }
}