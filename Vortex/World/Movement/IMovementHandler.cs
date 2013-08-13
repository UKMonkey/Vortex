using Vortex.Interface.EntityBase;

namespace Vortex.World.Movement
{
    public interface IMovementHandler
    {
        /** Used to provide the handler information about the world
         */
        World World {set;}

        /** used to establish if the entity is mobile or not
         */
        bool IsEntityMoving(Entity item);

        /** returns true if the entity was moved
         */
        bool HandleEntityMovement(Entity item);
    }
}
