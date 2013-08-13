using System.Collections.Generic;
using Vortex.Interface.EntityBase;
using Vortex.Interface.World.Chunks;

namespace Vortex.Interface.World.Entities
{
    public delegate void EntitiesCallback(List<Entity> updated);

    public interface IEntityCache
    {
        /// <summary>
        /// Called when an entity is loaded or generated.
        /// </summary>
        event EntitiesCallback OnEntitiesLoaded;

        /// <summary>
        /// Called when an entity is updated.
        /// </summary>
        event EntitiesCallback OnEntitiesUpdated;

        /// <summary>
        /// Called when an entity is deleted
        /// </summary>
        event EntitiesCallback OnEntitiesDeleted;

        /// <summary>
        /// get static entities in the given chunk
        /// </summary>
        /// <param name="area"></param>
        /// <returns></returns>
        IEnumerable<Entity> GetStaticEntities(ChunkKey area);

        /// <summary>
        /// get mobile entities in the given chunk
        /// </summary>
        /// <param name="area"></param>
        /// <returns></returns>
        IEnumerable<Entity> GetMobileEntities(ChunkKey area);

        /// <summary>
        /// Get entities in the given chunk
        /// </summary>
        /// <param name="area"></param>
        IEnumerable<Entity> GetEntities(ChunkKey area);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        ICollection<Entity> GetObservedEntities();

        /// <summary>
        /// Save entities in the given chunk.  Any entity already existing with the same id will be replaced.
        /// </summary>
        /// <param name="area"></param>
        void SaveEntities(ICollection<Entity> area);

        /// <summary>
        /// Update the entity to be deleted
        /// </summary>
        /// <param name="toDelete"></param>
        void DeleteEntity(int toDelete);

        /// <summary>
        /// add entities to the cache
        /// </summary>
        /// <param name="toAdd"></param>
        void AddEntities(List<Entity> toAdd);

        /// <summary>
        /// update entities in the cache
        /// </summary>
        /// <param name="toAdd"></param>
        void UpdateEntities(List<Entity> toAdd);

        /// <summary>
        /// Any entities that have been loaded are available here
        /// </summary>
        void ProcessLoadedData();
    }
}
