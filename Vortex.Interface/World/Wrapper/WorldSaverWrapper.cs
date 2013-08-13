using System.Collections.Generic;
using Vortex.Interface.EntityBase;
using Vortex.Interface.World.Chunks;
using Vortex.Interface.World.Entities;
using Vortex.Interface.World.Triggers;

namespace Vortex.Interface.World.Wrapper
{
    public class WorldSaverWrapper : IWorldSaver
    {
        protected readonly List<IEntitySaver> EntitySavers;
        protected readonly List<IChunkSaver> ChunkSavers;
        protected readonly List<ITriggerSaver> TriggerSavers;

        public WorldSaverWrapper()
        {
            EntitySavers = new List<IEntitySaver>();
            ChunkSavers = new List<IChunkSaver>();
            TriggerSavers = new List<ITriggerSaver>();
        }

        public WorldSaverWrapper(List<IEntitySaver> entitySavers,
                                List<IChunkSaver> chunkSavers,
                                List<ITriggerSaver> triggerSavers)
            :this()
        {
            EntitySavers = entitySavers;
            ChunkSavers = chunkSavers;
//            TriggerSavers = triggerSavers;
        }

        public WorldSaverWrapper(IEntitySaver entitySaver,
                                IChunkSaver chunkSaver,
                                ITriggerSaver triggerSaver)
            : this()
        {
            EntitySavers = new List<IEntitySaver>{entitySaver};
            ChunkSavers = new List<IChunkSaver>{chunkSaver};
//            TriggerSavers = new List<ITriggerSaver>{triggerSaver};
        }

        public void Dispose()
        {
            foreach (var item in EntitySavers)
                item.Dispose();
            foreach (var item in ChunkSavers)
                item.Dispose();
//            foreach (var item in TriggerSavers)
//                item.Dispose();
        }

        public void SaveTrigger(List<ITrigger> toSave)
        {
//            foreach (var item in TriggerSavers)
//                item.SaveTrigger(toSave);
        }

        public void SaveChunks(List<Chunk> chunksToSave)
        {
            foreach (var item in ChunkSavers)
                item.SaveChunks(chunksToSave);
        }

        public void SaveEntities(ICollection<Entity> entities)
        {
            foreach (var item in EntitySavers)
                item.SaveEntities(entities);
        }

        public void DeleteEntity(Entity entities)
        {
            foreach (var item in EntitySavers)
                item.DeleteEntity(entities);
        }
    }
}
