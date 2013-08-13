using System;
using System.Collections.Generic;
using Vortex.Interface.EntityBase;

namespace Vortex.Interface.World.Entities
{
    public interface IEntitySaver : IDisposable
    {
        /** store the entities somehow
         */
        void SaveEntities(ICollection<Entity> entities);

        /** remove the entity if it's saved
         */
        void DeleteEntity(Entity entities);
    }
}
