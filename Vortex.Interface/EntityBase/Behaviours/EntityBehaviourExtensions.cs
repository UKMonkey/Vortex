namespace Vortex.Interface.EntityBase.Behaviours
{
    public static class EntityBehaviourExtensions
    {
        public static IEntityFactory RegisterBehaviour(this IEntityFactory factory, short entityId, EntityBehaviourEnum behaviourId, IEntityBehaviour behaviour)
        {
            return factory.RegisterBehaviour(entityId, (short)behaviourId, behaviour);
        }

        public static void PerformBehaviour(this Entity entity, EntityBehaviourEnum behaviourId, Entity instagator)
        {
            entity.PerformBehaviour((short)behaviourId, instagator);
        }

        public static void Think(this Entity entity)
        {
            entity.PerformBehaviour(EntityBehaviourEnum.Think, null);
        }

        public static void PlayerChanged(this Entity entity)
        {
            entity.PerformBehaviour(EntityBehaviourEnum.PlayerChanged, null);
        }

        public static void OnSpawn(this Entity entity)
        {
            entity.PerformBehaviour(EntityBehaviourEnum.OnSpawn, null);
        }

        public static void OnInteract(this Entity entity, Entity instigator)
        {
            entity.PerformBehaviour(EntityBehaviourEnum.OnInteract, instigator);
        }
    }
}
