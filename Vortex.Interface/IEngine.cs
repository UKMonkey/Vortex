using System;
using System.Collections.Generic;
using Psy.Core;
using Psy.Core.Collision;
using Psy.Core.Console;
using Psy.Graphics.Models;
using SlimMath;
using Vortex.Interface.Audio;
using Vortex.Interface.EntityBase;
using Vortex.Interface.Net;
using Vortex.Interface.World;
using Vortex.Interface.World.Chunks;
using Vortex.Interface.World.Entities;

namespace Vortex.Interface
{
    public delegate void MessageHandler(Message msg);
    public delegate bool SpawnTest(IEngine engine, float outdoorLightIntensity, Color4 bakedLight, float fieldOfFiew, Vector3 position);
    
    public interface IEngine : IDisposable
    {
        int UpdateWorldFrequency { get; }
        int SynchronizeFrequency { get; }
        ITimeOfDayProvider TimeOfDayProvider { get; }
        uint CurrentFrameNumber { get; }

        IEnumerable<Entity> Entities { get; }
        IEntityFactory EntityFactory { get; }
        NetworkStatus NetworkStatus { get; }
        MaterialCache MaterialCache { get; }
        CompiledModelCache CompiledModelCache { get; }
        IMapGeometry MapGeometry { get; }

        IAudioLookup AudioLookup { get; }

        RemotePlayerCache RemotePlayers { get; }

        IEnumerable<Entity> GetEntitiesInChunk(ChunkKey chunkKey);
        IEnumerable<Entity> GetEntitiesWithinArea(Vector3 centre, float distance);

        /// <summary>
        /// Write text to the console with the specified Colour
        /// </summary>
        /// <param name="text"></param>
        /// <param name="colour"></param>
        void ConsoleText(string text, Color4 colour);

        void ConsoleText(string text);

        Entity GetEntity(int entityId);
        IEnumerable<Entity> GetEntities(IEnumerable<int> entityIds);

        /// <summary>
        /// Load the requested map into the engine. This will flush
        /// all map objects also.
        /// </summary>
        void LoadMap();

        Color4 OutsideLightingColour { get; set; }
        bool IsRaining { get; set; }

        /// <summary>
        /// List if connected clients
        /// </summary>
        IEnumerable<RemotePlayer> ConnectedClients { get; }

        /// <summary>
        /// Quake console - for executing console commands.
        /// </summary>
        IConsole Console { get; }

        void LoadMapCompleted();

        /// <summary>
        /// Server - send a message to all clients.
        /// Client - send a message to the server.
        /// </summary>
        /// <param name="message"></param>
        void SendMessage(Message message);

        /// <summary>
        /// spawn an entity of the given type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="numberToSpawn"></param>
        /// <returns></returns>
        IEnumerable<Entity> SpawnEntityAtRandomObservedLocation(short type, int numberToSpawn);
        IEnumerable<Entity> SpawnEntityAtRandomObservedLocation(short type, ChunkKey chunkKey, int numberToSpawn);
        IEnumerable<Entity> SpawnEntityAtRandomObservedLocation(short type, Vector3 centre, float distance, int numberToSpawn);

        void BroadcastSay(string messageString);
        void SendRconCommand(string command, string password);

        CollisionResult TraceRay(Vector3 from, float angle, Func<Entity, bool> filter);
        CollisionResult TraceRay(Vector3 source, Vector3 target, Func<Entity, bool> filter);
        CollisionResult TraceRay(Vector3 source, Vector3 target, IEnumerable<Entity> entities);
        CollisionResult TraceRay(Ray ray, Func<Entity, bool> filter);

        /// <summary>
        /// Register a custom network message handler
        /// </summary>
        /// <param name="msgType"></param>
        /// <param name="handler"></param>
        void RegisterMessageCallback(Type msgType, MessageHandler handler);

        /// <summary>
        /// Unregister custom network message handler
        /// </summary>
        /// <param name="msgType"></param>
        void UnregisterMessageCallback(Type msgType);



        /**************/
        /// Entities
        /**************/


        event EntitiesCallback OnEntitiesGenerated;

        /// <summary>
        /// register a spawn requirement for a given (entity) type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="test"></param>
        void RegisterRequirement(short type, SpawnTest test);

        /// <summary>
        /// Converts a Chunk vector in a given chunk to be a world vector
        /// </summary>
        /// <param name="chunkKey"></param>
        /// <param name="vector"></param>
        /// <returns></returns>
        Vector3 ChunkVectorToWorldVector(ChunkKey chunkKey, Vector3 vector);

        /// <summary>
        /// Server: Track entities moving around.  This should be called on every entity that should be kept observed.
        /// </summary>
        /// <param name="thingToTrack"></param>
        void TrackEntity(Entity thingToTrack);

        /// <summary>
        /// Server: Stop tracking an entity moving around
        /// </summary>
        /// <param name="thingToStopTracking"></param>
        void StopTrackingEntity(Entity thingToStopTracking);
    }
}
