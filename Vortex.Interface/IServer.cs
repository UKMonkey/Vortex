using SlimMath;
using Vortex.Interface.EntityBase;
using Vortex.Interface.Net;
using Vortex.Interface.World.Chunks;

namespace Vortex.Interface
{
    public interface IServer : IEngine
    {
        float ObservedSize { get; }

        IConsoleCommandContext ConsoleCommandContext { get; }

        /// <summary>
        /// Returns the entity controlled by the current remote player.
        /// </summary>
        /// <param name="remotePlayer"></param>
        Entity GetEntity(RemotePlayer remotePlayer);

        /// <summary>
        /// Returns the entity for a player specified by name (case insensitive).
        /// </summary>
        /// <param name="playerName"></param>
        Entity GetEntity(string playerName);

        void Listen(int port);

        /// <summary>
        /// Server - send a message to all clients bar the excepted one.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="except"></param>
        void SendMessage(Message message, RemotePlayer except);

        /// <summary>
        /// Server - send message to a single client.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="destination"></param>
        void SendMessageToClient(Message message, RemotePlayer destination);

        /// <summary>
        /// Tell the client which entity it is to follow
        /// </summary>
        /// <param name="player"></param>
        /// <param name="entity"></param>
        void SetClientFocus(RemotePlayer player, Entity entity);

        /// <summary>
        /// Add the entity to the world.
        /// </summary>
        /// <param name="entity"></param>
        void SpawnEntity(Entity entity);

        /// <summary>
        /// Gives the chunk key that the given vector points to
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        ChunkKey GetChunkKeyForWorldVector(Vector3 vector);

        /// <summary>
        /// Gives the chunkVector for the given world vector
        /// </summary>
        /// <param name="worldVector"></param>
        /// <param name="key"></param>
        /// <param name="chunkVector"></param>
        void GetChunkVectorForWorldVector(Vector3 worldVector, out ChunkKey key, out Vector3 chunkVector);

        /// <summary>
        /// Play a sound at an entities position.
        /// </summary>
        /// <param name="target">Target entity</param>
        /// <param name="audioChannel">See AudioChannel enum</param>
        /// <param name="soundFilename">See Sounds static class</param>
        void PlaySoundOnEntity(Entity target, byte audioChannel, string soundFilename);
    }
}