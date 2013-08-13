using Psy.Core.Input;
using SlimMath;
using Vortex.Interface.EntityBase;
using Vortex.Interface.Net;

namespace Vortex.Interface
{
    public interface IGameClient : IGame
    {
        /// <summary>
        /// OnAttach the engine to the game.
        /// </summary>
        /// <param name="engine"></param>
        void OnAttach(IClient engine);

        /// <summary>
        /// Event raised when the mouse button is clicked on the window
        /// </summary>
        /// <param name="args"></param>
        void OnScreenMouseButtonDown(MouseEventArguments args);

        /// <summary>
        /// Mouse cursor moved over the world map.
        /// </summary>
        /// <param name="viewCoords"></param>
        void OnWorldMouseMove(Vector3 viewCoords);

        void OnFocusChange(Entity entity);

        /// <summary>
        /// Client method - called when a client joins
        /// </summary>
        /// <param name="player"></param>
        void OnClientJoin(RemotePlayer player);

        /// <summary>
        /// Called when the client failed to connect to the server
        /// </summary>
        /// <param name="reason"></param>
        void OnClientRejected(RejectionReasonEnum reason);

        /// <summary>
        /// Client method - called when a client leaves
        /// </summary>
        /// <param name="player"></param>
        void OnClientLeave(RemotePlayer player);

        /// <summary>
        /// Client method - called when a definitive starting client list is received
        /// </summary>
        void OnConnectedClientListRecieved();

        /// <summary>
        /// Get the current player name.
        /// </summary>
        string PlayerName { get; }
    }
}