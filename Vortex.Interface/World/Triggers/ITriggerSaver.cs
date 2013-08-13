using System;
using System.Collections.Generic;

namespace Vortex.Interface.World.Triggers
{
    public interface ITriggerSaver : IDisposable
    {
        /** saves the trigger somehow
         */
        void SaveTrigger(List<ITrigger> toSave);
    }
}
