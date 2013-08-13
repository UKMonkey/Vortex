using System.Collections.Generic;
using Vortex.Interface.World.Chunks;

namespace Vortex.Interface.World.Triggers
{
    public interface ITriggerCache
    {
        event TriggerCallback OnTriggerLoaded;
        event TriggerCallback OnTriggerUpdated;

        /// <summary>
        /// Get entities in the given chunk
        /// </summary>
        /// <param name="area"></param>
        List<ITrigger> GetTriggers(ChunkKey area);

        /// <summary>
        /// Update the entity in cache and do whatever is required
        /// </summary>
        /// <param name="toUpdate"></param>
        void UpdateTriggers(List<ITrigger> toUpdate);
    }
}
