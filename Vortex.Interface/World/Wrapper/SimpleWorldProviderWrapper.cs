using System.Collections.Generic;
using Vortex.Interface.World.Chunks;
using Vortex.Interface.World.Entities;
using Vortex.Interface.World.Triggers;

namespace Vortex.Interface.World.Wrapper
{
    public class SimpleWorldProviderWrapper : WorldProviderWrapper
    {
        public SimpleWorldProviderWrapper(IChunkLoader chunkLoader, IEntityLoader entityLoader, ITriggerLoader triggerLoader)
        {
            RegisterChunkEvents(chunkLoader);
            RegisterEntityEvents(entityLoader);
            RegisterTriggerEvents(triggerLoader);
        }

        public override void LoadChunks(List<ChunkKey> chunkKeys)
        {
            ChunkProviders[0].LoadChunks(chunkKeys);
        }

        public override void LoadTriggers(ChunkKey location)
        {
            TriggerProviders[0].LoadTriggers(location);
        }

        public override void LoadEntities(ChunkKey area)
        {
            EntityProviders[0].LoadEntities(area);
        }

        public override void LoadEntities(List<ChunkKey> area)
        {
            EntityProviders[0].LoadEntities(area);
        }
    }
}
