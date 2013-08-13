using System.Collections.Generic;
using SlimMath;

namespace Vortex.Interface.World.Triggers
{
    public interface ITriggerFactory
    {
        ITrigger GetTrigger(string type, TriggerKey key, Vector3 position);
        ITrigger GetTrigger(string type, TriggerKey key, Vector3 position, IEnumerable<KeyValuePair<string, string>> properties);
    }
}
