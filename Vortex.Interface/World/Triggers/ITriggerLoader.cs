using System;
using System.Collections.Generic;
using Vortex.Interface.World.Chunks;

namespace Vortex.Interface.World.Triggers
{
    public delegate void TriggerCallback(ChunkKey key, List<ITrigger> trigger);

    public interface ITriggerLoader : IDisposable
    {
        /** called when a trigger is generated from scratch
         */
        event TriggerCallback OnTriggerGenerated;

        /** called when a trigger has been loaded
         */
        event TriggerCallback OnTriggerLoaded;

        /**  Called when this loader is unable to get triggers in a certain area
         */
        event ChunkKeyCallback OnTriggersUnavailable;

        /** load the triggers in a given area
         */
        void LoadTriggers(ChunkKey location);
    }
}
