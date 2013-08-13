using System.Collections.Generic;
using SlimMath;

namespace Vortex.Interface.World.Triggers
{
    public delegate void TriggerActivated(ITrigger trigger);

    public interface ITrigger
    {
        /// <summary>
        /// Name of the trigger.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Called when the trigger is activated.
        /// </summary>
        TriggerActivated OnActivated { get; set; }

        /// <summary>
        /// Unique key.
        /// </summary>
        TriggerKey UniqueKey { get; }

        /// <summary>
        /// Trigger location.
        /// </summary>
        Vector3 Location { get; }

        /// <summary>
        /// How the trigger is activated.
        /// </summary>
        TriggerActivationMethod ActivationMethod { get; }

        /// <summary>
        /// Should this trigger be known to the client?
        /// </summary>
        bool SendToClient { get; }

        /// <summary>
        /// Test to see if this trigger should activate
        /// </summary>
        void Update();

        void SetProperties(TriggerKey key, Vector3 location, IEnumerable<KeyValuePair<string, string>> properties);

        /// <summary>
        /// Get trigger properties for serialization.
        /// </summary>
        /// <returns></returns>
        IDictionary<string, string> GetProperties();
    }
}
