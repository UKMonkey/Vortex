using System.Collections.Generic;
using Vortex.Interface.EntityBase;
using Vortex.Interface.World.Chunks;
using Vortex.Interface.World.Entities;
using Vortex.Interface.World.Triggers;

namespace Vortex.Interface.World.Wrapper
{
    public abstract class WorldProviderWrapper : IWorldProvider
    {
        public event ChunkCallback OnChunkLoad;
        public event ChunkCallback OnChunksGenerated;
        public event ChunkKeyCallback OnChunksUnavailable;

        public event EntityChunkKeyCallback OnEntityGenerated;
        public event EntityChunkKeyCallback OnEntityLoaded;
        public event EntitiesCallback OnEntityUpdated;
        public event EntityIdCallback OnEntityDeleted;
        public event ChunkKeyCallback OnEntitiesUnavailable;

        public event TriggerCallback OnTriggerGenerated;
        public event TriggerCallback OnTriggerLoaded;
        public event ChunkKeyCallback OnTriggersUnavailable;

        protected readonly List<IChunkLoader> ChunkProviders;
        protected readonly List<IEntityLoader> EntityProviders;
        protected readonly List<ITriggerLoader> TriggerProviders;

        protected WorldProviderWrapper()
        {
            ChunkProviders = new List<IChunkLoader>();
            EntityProviders = new List<IEntityLoader>();
            TriggerProviders = new List<ITriggerLoader>();
        }

        public void Dispose()
        {
            foreach (var item in ChunkProviders)
                item.Dispose();
            foreach (var item in EntityProviders)
                item.Dispose();
            foreach (var item in TriggerProviders)
                item.Dispose();
        }

        protected void RegisterEvents(IWorldProvider item)
        {
            RegisterChunkEvents(item);
            RegisterTriggerEvents(item);
            RegisterEntityEvents(item);
        }

        protected void RegisterEntityEvents(IEnumerable<IEntityLoader> entityProviders)
        {
            foreach (var item in entityProviders)
                RegisterEntityEvents(item);
        }

        protected void RegisterChunkEvents(IEnumerable<IChunkLoader> chunkProviders)
        {
            foreach (var item in chunkProviders)
                RegisterChunkEvents(item);
        }

        protected void RegisterTriggerEvents(IEnumerable<ITriggerLoader> triggerProviders)
        {
            foreach (var item in triggerProviders)
                RegisterTriggerEvents(item);
        }

        protected void RegisterChunkEvents(IChunkLoader item)
        {
            item.OnChunkLoad += ChunksLoaded;
            item.OnChunksGenerated += ChunksGenerated;
            item.OnChunksUnavailable += ChunksUnavailable;

            ChunkProviders.Add(item);
        }

        protected void RegisterTriggerEvents(ITriggerLoader item)
        {
            item.OnTriggerLoaded += TriggerLoaded;
            item.OnTriggersUnavailable += TriggersUnavailable;
            item.OnTriggerGenerated += TriggerGenerated;

            TriggerProviders.Add(item);
        }

        protected void RegisterEntityEvents(IEntityLoader item)
        {
            item.OnEntityDeleted += EntityDeleted;
            item.OnEntityGenerated += EntityGenerated;
            item.OnEntityLoaded += EntityLoaded;
            item.OnEntityUpdated += EntityUpdated;
            item.OnEntitiesUnavailable += EntitiesUnavailable;

            EntityProviders.Add(item);
        }


#region chunks
        public abstract void LoadChunks(List<ChunkKey> chunkKeys);

        protected virtual void ChunksLoaded(List<Chunk> chunks)
        {
            if (OnChunkLoad != null)
                OnChunkLoad(chunks);
        }

        protected virtual void ChunksGenerated(List<Chunk> chunks)
        {
            if (OnChunksGenerated != null)
                OnChunksGenerated(chunks);
        }

        protected virtual void ChunksUnavailable(List<ChunkKey> chunks)
        {
            if (OnChunksUnavailable != null)
                OnChunksUnavailable(chunks);
        }
#endregion


#region triggers
        public abstract void LoadTriggers(ChunkKey location);

        protected virtual void TriggerGenerated(ChunkKey key, List<ITrigger> triggers)
        {
            if (OnTriggerGenerated != null)
                OnTriggerGenerated(key, triggers);
        }

        protected virtual void TriggerLoaded(ChunkKey key, List<ITrigger> triggers)
        {
            if (OnTriggerLoaded != null)
                OnTriggerLoaded(key, triggers);
        }

        protected virtual void TriggersUnavailable(List<ChunkKey> keys)
        {
            if (OnTriggersUnavailable != null)
                OnTriggersUnavailable(keys);
        }
#endregion


 #region entities
        public abstract void LoadEntities(ChunkKey area);
        public abstract void LoadEntities(List<ChunkKey> area);

        protected virtual void EntityDeleted(List<int> deleted)
        {
            if (OnEntityDeleted != null)
                OnEntityDeleted(deleted);
        }

        protected virtual void EntityLoaded(List<Entity> loaded, ChunkKey area)
        {
            if (OnEntityLoaded != null)
                OnEntityLoaded(loaded, area);
        }

        protected virtual void EntityGenerated(List<Entity> loaded, ChunkKey area)
        {
            if (OnEntityGenerated != null)
                OnEntityGenerated(loaded, area);
        }

        protected virtual void EntityUpdated(List<Entity> updated)
        {
            if (OnEntityUpdated != null)
                OnEntityUpdated(updated);
        }

        protected virtual void EntitiesUnavailable(List<ChunkKey> unavailable)
        {
            if (OnEntitiesUnavailable != null)
                OnEntitiesUnavailable(unavailable);
        }
#endregion
    }
}
