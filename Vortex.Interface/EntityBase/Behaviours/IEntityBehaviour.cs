namespace Vortex.Interface.EntityBase.Behaviours
{
    public interface IEntityBehaviour
    {
        /** instigator is the entity that triggered this behaviour - if applicable
         *  target is the entity that is to be processed in this collision
         *  
         *  note that this behaviour will be performed twice; once with the entities one way around, then again with them the other.
         */
        void PerformBehaviour(Entity target, Entity instigator);
    }
}
