using Vortex.Interface.EntityBase;
using Vortex.Interface.Net;
using Vortex.Interface.World.Wrapper;

namespace Vortex.Interface
{
    public struct WorldDataHandler
    {
        public IWorldProvider WorldProvider { get; set; }
        public IWorldSaver WorldSaver { get; set; }
    }

    public interface IGameServer : IGame
    {
        IServer Engine { get; }

        WorldDataHandler GetWorldHandling();

        /// <summary>
        /// OnAttach the engine to the game.
        /// </summary>
        /// <param name="engine"></param>
        void OnAttach(IServer engine);

        /// <summary>
        /// A new client has joined the game.
        /// </summary>
        /// <param name="player"></param>
        void OnClientConnected(RemotePlayer player);

        /// <summary>
        /// A client has left (timeout or intentional)
        /// </summary>
        /// <param name="player"></param>
        void OnClientDisconnected(RemotePlayer player);

        /// <summary>
        /// Server method - respawn a client.
        /// </summary>
        /// <param name="player"></param>
        Entity SpawnPlayer(RemotePlayer player);
    }
}