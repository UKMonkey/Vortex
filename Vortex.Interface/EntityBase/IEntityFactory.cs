using Vortex.Interface.EntityBase.Behaviours;
using Vortex.Interface.EntityBase.Damage;
using Vortex.Interface.EntityBase.Properties;

namespace Vortex.Interface.EntityBase
{
    public interface IEntityFactory
    {
        // Add a new entity type to the factory and say how it should be created
        IEntityFactory Add(short entityTypeId, string name);
        
        // for the entity type last added, register a behaviour with it
        IEntityFactory RegisterBehaviour(short entityTypeId, short behaviourId, IEntityBehaviour behaviour);

        // for the entity type last added, register a default method to handle damage
        IEntityFactory RegisterDefaultDamageHandler(short entityTypeId, IEntityDamageHandler handler);

        // for the entity type last added, use the given handler in preference over the default
        IEntityFactory RegisterDamageHandler(short entityTypeId, IEntityDamageHandler handler, DamageTypeEnum type);

        // for the entity type last added, register a default property for it to be made with
        IEntityFactory RegisterDefaultProperty(EntityProperty property);

        // return a new instance of the given entity type
        Entity Get(short entityTypeId);

        // return the number of entity types registered
        int GetRegisteredCount();
    }
}