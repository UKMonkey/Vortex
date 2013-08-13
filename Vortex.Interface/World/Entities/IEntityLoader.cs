using System;
using System.Collections.Generic;
using Vortex.Interface.EntityBase;
using Vortex.Interface.World.Chunks;

namespace Vortex.Interface.World.Entities
{
    public delegate void EntityIdCallback(List<int> id);
    public delegate void EntityChunkKeyCallback(List<Entity> entities, ChunkKey area);

    public interface IEntityLoader : IDisposable
    {
        event EntityChunkKeyCallback OnEntityGenerated;
        event EntityChunkKeyCallback OnEntityLoaded;
        event EntitiesCallback OnEntityUpdated;
        event EntityIdCallback OnEntityDeleted;
        event ChunkKeyCallback OnEntitiesUnavailable;

        /** Load all the entities
         */
        void LoadEntities(ChunkKey area);

        /** As above, but it might be faster in some cases to provide multiple chunks rather
         * than 1 at a time...
         */
        void LoadEntities(List<ChunkKey> area);
    }
}
