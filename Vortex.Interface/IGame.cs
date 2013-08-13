using Vortex.Interface.EntityBase;
using Vortex.Interface.Net;

namespace Vortex.Interface
{
    public interface IGame
    {
        IEngine Engine { get; }
        string Version { get; }
        string GameName { get; }
        IEntityFactory EntityFactory { get; }

        /// <summary>
        /// Event raised when the game is launched
        /// </summary>
        void OnBegin();

        /// <summary>
        /// The network has changed state. Disconnected by default.
        /// </summary>
        /// <param name="networkStatus"></param>
        void OnNetworkStatusChanged(NetworkStatus networkStatus);

        /// <summary>
        /// Update tick.
        /// </summary>
        void UpdateWorld();
    }
}
