using System;
using System.Collections.Generic;
using System.Linq;
using Lidgren.Network;
using Psy.Core;
using Psy.Core.Collision;
using Psy.Core.Configuration;
using Psy.Core.Console;
using Psy.Core.Logging;
using Psy.Core.Tasks;
using Psy.Graphics.Models;
using SlimMath;
using Vortex.Audio;
using Vortex.Entities;
using Vortex.Interface;
using Vortex.Interface.Audio;
using Vortex.Interface.Debugging;
using Vortex.Interface.EntityBase;
using Vortex.Interface.Net;
using Vortex.Interface.World;
using Vortex.Interface.World.Chunks;
using Vortex.Interface.World.Entities;
using Vortex.Interface.World.Triggers;
using Vortex.Net;
using Vortex.Net.Messages;
using Vortex.World;
using Vortex.World.Movement;

namespace Vortex
{
    public abstract class EngineBase<T> : IEngine where T : IGame
    {
        private bool _skipNextUpdate = false;
        public int CacheClearingFrequency { get { return 1000; } }
        public int UpdateWorldFrequency { get { return 30; } }
        public int SynchronizeFrequency { get { return 20; } }
        public int UpdateNetworkFrequency { get { return 5; } }
        public uint CurrentFrameNumber { get; private set; }

        public ITimeOfDayProvider TimeOfDayProvider { get { return World; } }

        private readonly IAudioLookup _audioLookup;
        public IAudioLookup AudioLookup { get { return _audioLookup; } }

        private readonly RemotePlayerCache _remotePlayerCache;
        public RemotePlayerCache RemotePlayers { get { return _remotePlayerCache; } }

        public T Game { get; set; }
        protected readonly StartArguments EngineInitArgs;

        public static string Version{ get { return typeof (EngineBase<T>).Assembly.GetName().Version.ToString(); } }
        public static string ModName{ get { return StaticConfigurationManager.ConfigurationManager.GetString("ModServerDLL"); } }

        public IEnumerable<RemotePlayer> ConnectedClients{get { return RemotePlayers.GetPlayers(); }}
        public virtual IConsole Console { get { return null; } }

        protected EngineBase(StartArguments args)
        {
            _callbacks = new Dictionary<Type, MessageHandler>();
            CurrentFrameNumber = 0;

            EngineInitArgs = args;
            NetworkStatus = NetworkStatus.Disconnected;

            StaticTaskQueue.TaskQueue.CreateRepeatingTask("EngineBase.UpdateWorld", UpdateWorld, UpdateWorldFrequency);
            StaticTaskQueue.TaskQueue.CreateRepeatingTask("EngineBase.UpdateNetworking", UpdateNetworking, UpdateNetworkFrequency);
            
            SpawnTests = new Dictionary<short, List<SpawnTest>>();
            MaterialCache = new MaterialCache();
            CompiledModelCache = new CompiledModelCache(new MaterialTranslator(MaterialCache), MaterialCache);
            
            _remotePlayerCache = new RemotePlayerCache();
            _audioLookup = new AudioLookup();
        }

        public void Dispose()
        {
            Logger.Write("Disposing of the engine");
            if (World != null) 
                World.Dispose();
            World = null;
        }

        public virtual void ConsoleText(string text, Color4 colour) { }
        public virtual void ConsoleText(string text) { }

        protected abstract void AttachMod();
        public void AttachModule()
        {
            EntityFactory = new EntityFactory(CompiledModelCache);

            AttachMod();

            MsgIdFactory = new MessageIdFactory();
            StaticTriggerFactory.Instance = new TriggerFactory(this);
        }

        /***********************************************/
        #region Entities
        /***********************************************/

        public event EntitiesCallback OnEntitiesGenerated;
        public IEnumerable<Entity> Entities { get { return World != null ? World.GetObservedEntities() : new List<Entity>(); } }
        public IEntityFactory EntityFactory { get; private set; }
        private Dictionary<short, List<SpawnTest>> SpawnTests { get; set; }

        public Entity GetEntity(int entityId)
        {
            return World.GetEntity(entityId);
        }

        public IEnumerable<Entity> GetEntities(IEnumerable<int> entityIds)
        {
            return entityIds.Select(GetEntity).Where(item => item != null);
        }

        /// <summary>
        /// Get all entities within a specific chunk.
        /// </summary>
        /// <param name="chunkKey"></param>
        /// <returns></returns>
        public IEnumerable<Entity> GetEntitiesInChunk(ChunkKey chunkKey)
        {
            return World.GetEntitiesInChunk(chunkKey);
        }

        /// <summary>
        /// Get all entities that are a certain distance from the given point.
        /// </summary>
        /// <param name="centre"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public IEnumerable<Entity> GetEntitiesWithinArea(Vector3 centre, float distance)
        {
            return World.GetEntitiesWithinArea(centre, distance);
        }

        /// <summary>
        /// Register a requirement for spawning an entity type.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="test"></param>
        public void RegisterRequirement(short type, SpawnTest test)
        {
            if (!SpawnTests.ContainsKey(type))
                SpawnTests.Add(type, new List<SpawnTest>());

            SpawnTests[type].Add(test);
        }

        /// <summary>
        /// Spawn an entity of the given type.
        /// Tests are expected to already be registered to test where the entity can be spawned
        /// The entity produced is not registered with the system; allowing the game to accept or
        /// reject the new entity
        /// </summary>
        /// <param name="type"></param>
        /// <param name="chunkKey"></param>
        /// <param name="numberToSpawn"></param>
        /// <returns></returns>
        public IEnumerable<Entity> SpawnEntityAtRandomObservedLocation(short type, ChunkKey chunkKey, int numberToSpawn)
        {
            const float distance = Chunk.ChunkWorldSize/2;

            var location = ChunkVectorToWorldVector(chunkKey, new Vector3(distance, distance, 0));
            return SpawnEntityAtRandomObservedLocation(type, location, distance, numberToSpawn);
        }

        public IEnumerable<Entity> SpawnEntityAtRandomObservedLocation(short type, Vector3 location, float distance, int countToSpawn)
        {
            var tests = new List<SpawnTest>();
            try
            {
                tests.AddRange(SpawnTests[type]);
            }
            catch (Exception)
            {
                throw new Exception("No tests specified for type - unable to spawn in a random location" + type);
            }

            var tester = new ProximityTester(location, distance);
            tests.Insert(0, tester.Test);

            var spawned = SpawnEntityAtRandomPosition(type, tests, countToSpawn);

            return spawned;
        }

        /// <summary>
        /// Spawn an entity of the given type.
        /// Tests are expected to already be registered to test where the entity can be spawned
        /// The entity produced is not registered with the system; allowing the game to accept or
        /// reject the new entity
        /// </summary>
        /// <param name="type"></param>
        /// <param name="countToSpawn"></param>
        /// <returns></returns>
        public IEnumerable<Entity> SpawnEntityAtRandomObservedLocation(short type, int countToSpawn)
        {
            List<SpawnTest> tests;
            try
            {
                tests = SpawnTests[type];
            }
            catch (Exception)
            {
                throw new Exception("No tests specified for type - unable to spawn in a random location" + type);
            }

            return SpawnEntityAtRandomPosition(type, tests, countToSpawn);
        }

        private List<Entity> SpawnEntityAtRandomPosition(short type, List<SpawnTest> tests, int countToSpawn)
        {
            var ret = new List<Entity>();
            if (countToSpawn == 0)
                return ret;

            var availableKeys = World.GetMap().GetObservedChunkKeys().ToList();

            if (availableKeys.Count == 0)
                return ret;
            
            for (var i=0; i<countToSpawn; ++i)
            {
                var chunkKeyIndex = StaticRng.Random.Next(0, availableKeys.Count-1);
                var chunk = availableKeys[chunkKeyIndex];
                var chunkVector = new Vector3(
                    (float) StaticRng.Random.NextDouble()*Chunk.ChunkWorldSize,
                    (float) StaticRng.Random.NextDouble()*Chunk.ChunkWorldSize,
                    0);
                var spot = ChunkVectorToWorldVector(chunk, chunkVector);
                var rotation = StaticRng.Random.Next(0, 360) / 360f;

                var entity = CreateEntity(spot, rotation, type, 0);
                if (entity != null)
                    ret.Add(entity);
            }

            World.AddEntities(ret);
            return ret;
        }

        protected Entity CreateEntity(Vector3 position, float rotation, short type, short parent)
        {
            var entity = EntityFactory.Get(type);
            entity.SetPosition(position);
            entity.SetRotation(rotation);
            entity.Parent = parent;

            return entity;
        }


        /***********************************************/
        #endregion
        /***********************************************/

        /***********************************************/
        #region messages
        /***********************************************/

        public NetworkStatus NetworkStatus { get; protected set; }

        private readonly Dictionary<Type, MessageHandler> _callbacks;
        private MessageIdFactory MsgIdFactory { get; set; }
        private Dictionary<Type, MessageHandler> MessageCallbacks { get { return _callbacks; } }

        /// <summary>
        /// Update anything required on the network layer
        /// </summary>
        protected abstract void UpdateNetworking();

        /// <summary>
        /// register a handler to process a message type
        /// </summary>
        /// <param name="msgType"></param>
        /// <param name="handler"></param>
        public void RegisterMessageCallback(Type msgType, MessageHandler handler)
        {
            if (MessageCallbacks.ContainsKey(msgType))
            {
                throw new ApplicationException(
                    string.Format("Cannot bind an additional handler to network message '{0}'", msgType));
            }
            MessageCallbacks.Add(msgType, handler);
        }

        /// <summary>
        /// Unregister a handler to process a message type
        /// </summary>
        /// <param name="msgType"></param>
        public void UnregisterMessageCallback(Type msgType)
        {
            if (!MessageCallbacks.ContainsKey(msgType))
            {
                //throw new ApplicationException(string.Format("No such message handler bound for message {0}", msgType));
            }
            else
            {
                MessageCallbacks.Remove(msgType);
            }
        }

        protected virtual void OnMessage(string msgType, int byteLength)
        {}

        protected void OnMessage(NetIncomingMessage msg)
        {
            var stats = new TimingStats("Message processing");

            stats.StartingTask("Message deserialisation");
            var decodedMsg = DecodeMessage(msg);
            stats.CompletedTask("Message deserialisation");

            stats.StartingTask(string.Format("Message handling {0}", decodedMsg.GetType().Name));
            Logger.Write(String.Format("Processing message {0}", decodedMsg.GetType().Name), LoggerLevel.Trace);

            foreach (var submsg in decodedMsg.SubMessages())
                HandleMessage(submsg);

            HandleMessage(decodedMsg);
            stats.CompletedTask(string.Format("Message handling {0}", decodedMsg.GetType().Name));

            stats.LogStats(5, 20, 30);
        }

        protected Message DecodeMessage(NetIncomingMessage msg)
        {
            var messageTypeId = msg.ReadByte();
            var msgType = MsgIdFactory.GetMessageType(messageTypeId);

            var msgImpl = (Message)Activator.CreateInstance(msgType);
            var msgStream = new IncomingMessageStream(msg, this);
            msgImpl.Sender = msgStream.RemoteClient();
            msgImpl.Deserialize(msgStream);
            OnMessage(msgImpl.GetType().Name, msg.LengthBytes);

            return msgImpl;
        }

        protected virtual void HandleMessage(Message msg)
        {
            // assert we've read everything!
            // disabled as it can fail with a difference of <8 bits
            //Debug.Assert(msg.Position == msg.LengthBits);

            MessageHandler msgHandler;

            if (MessageCallbacks.TryGetValue(msg.GetType(), out msgHandler))
                msgHandler.Invoke(msg);
        }


        protected void ChangeNetworkStatus(NetworkStatus networkStatus)
        {
            Logger.Write(string.Format("Network status changed to {0}", networkStatus));
            NetworkStatus = networkStatus;
            Game.OnNetworkStatusChanged(NetworkStatus);
        }

        /**  Server - Send a message to all clients
         *   Client - Send a message to the server
         */
        public abstract void SendMessage(Message msg);

        
        protected NetOutgoingMessage SerializeMessage(Message msg)
        {
            var msgType = MsgIdFactory.GetMessageId(msg.GetType());
            var lidMsg = CreateNetworkMessage();
            var msgStream = new OutgoingMessageStream(lidMsg);

            msgStream.WriteByte(msgType);
            msg.Serialize(msgStream);
            return lidMsg;
        }

        protected abstract NetOutgoingMessage CreateNetworkMessage();

        /// <summary>
        /// Broadcast a "say" message to all connected clients
        /// </summary>
        /// <param name="messageString"></param>
        public virtual void BroadcastSay(string messageString) { }

        /// <summary>
        /// Send a remote command to be executed on the server
        /// </summary>
        /// <param name="command"></param>
        /// <param name="password"></param>
        public virtual void SendRconCommand(string command, string password) { }

        /***********************************************/
        #endregion
        /***********************************************/

        /***********************************************/
        #region World And Map
        /***********************************************/

        public World.World World { get; private set; }
        public IMapGeometry MapGeometry { get { return World.GetMap(); } }

        protected Map Map { get; private set; }
        public WorldDataCache DataCache { get; private set; }
        public MaterialCache MaterialCache { get; set; }
        public CompiledModelCache CompiledModelCache { get; set; }

        protected abstract WorldDataCache GetCachingStrategy();

        public Vector3 ChunkVectorToWorldVector(ChunkKey chunkKey, Vector3 vector)
        {
            return new Vector3(Chunk.ChunkWorldSize * chunkKey.X,
                                Chunk.ChunkWorldSize * chunkKey.Y, 0) + vector;
        }

        /**  Load the map & create the world & map
         */
        protected abstract IMovementHandler GetMovementHandler();

        public void LoadMap()
        {
            if (DataCache != null)
                return;

            IMovementHandler movementHandler = GetMovementHandler();
            DataCache = GetCachingStrategy();

            DataCache.OnEntitiesLoaded += NotifyGameOfNewEntities;

            World = new World.World(DataCache, movementHandler);
            Map = new Map(World, DataCache);
            World.UseMap(Map);

            LoadMapCompleted();
        }

        private void NotifyGameOfNewEntities(List<Entity> entities)
        {
            if (OnEntitiesGenerated != null)
                OnEntitiesGenerated(entities);
        }

        public Color4 OutsideLightingColour
        {
            get
            {
                return World == null ?
                    new Color4(1.0f, 0.0f, 0.0f, 0.0f) 
                    : World.OutsideLightingColour;
            }
            set
            {
                if (World != null) 
                    World.OutsideLightingColour = value;
            }
        }

        public bool IsRaining
        {
            get { return World != null && World.IsRaining; }
            set
            {
                if (World != null)
                {
                    if (World.IsRaining != value)
                    {
                        // changed.
                        var weatherChangeMessage = new ServerWeatherChangeMessage {IsRaining = value};
                        SendMessage(weatherChangeMessage);
                    }

                    World.IsRaining = value;
                }
            }
        }

        protected void UnloadMap()
        {
            if (World != null)
                World.Dispose();
            World = null;
        }

        protected IEnumerable<ChunkKey> GetObservedChunkKeys()
        {
            return
                Map == null
                    ? new List<ChunkKey>()
                    : Map.GetObservedChunkKeys();
        }

        public abstract void LoadMapCompleted();

        protected virtual void NextFrameStarted()
        {
        }

        private void UpdateWorld()
        {
            if (_skipNextUpdate)
            {
                _skipNextUpdate = false;
                return;
            }
            var stats = new TimingStats("EngineBase");
            CurrentFrameNumber++;
            NextFrameStarted();

            if (World != null)
            {
                stats.StartingTask("World.Update");
                stats.MergeStats(World.Update());
                stats.CompletedTask("World.Update");
            }

            stats.StartingTask("Game.Update");
            Game.UpdateWorld();
            stats.CompletedTask("Game.Update");

            stats.StartingTask("UpdateChunkCache");
            UpdateDataCache();
            stats.CompletedTask("UpdateChunkCache");

            stats.LogStats(30, 50, 70);
        }

        // it's a little bit of a lie, if we have to catch up, we'll do so
        // one frame at a time.  means we won't have any significant performance hit
        // and the engine will slowly get to the targeted frame...
        protected void FastForwardToFrame(uint frameId)
        {
            if (frameId > CurrentFrameNumber)
                UpdateWorld();
            else
                _skipNextUpdate = true;
        }

        private void UpdateDataCache()
        {
            if (DataCache != null)
            {
                DataCache.ProcessLoadedData();
            }
        }

        public virtual void TrackEntity(Entity thingToTrack)
        {
        }

        public virtual void StopTrackingEntity(Entity thingToStopTracking)
        {
        }

        /// <summary>
        /// follow a ray from the point at the given direction
        /// world only will ignore entities
        /// </summary>
        /// <param name="from"></param>
        /// <param name="angle"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public virtual CollisionResult TraceRay(Vector3 from, float angle, Func<Entity, bool> filter)
        {
            var direction = DirectionUtil.CalculateVector(angle);
            return World.TraceRay(from, direction, filter);
        }

        /// <summary>
        /// Trace a ray from a source to a target.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="filter">Return false to exclude an entity from the hit-test</param>
        /// <returns></returns>
        public virtual CollisionResult TraceRay(Vector3 source, Vector3 target, Func<Entity, bool> filter)
        {
            var direction = target - source;
            return World.TraceRay(source, direction, filter);
        }

        public CollisionResult TraceRay(Vector3 source, Vector3 target, IEnumerable<Entity> entities)
        {
            var direction = target - source;
            return World.TraceRay(source, direction, entities);
        }

        public CollisionResult TraceRay(Ray ray, Func<Entity, bool> filter)
        {
            return World.TraceRay(ray.Position, ray.Direction, filter);
        }

        /***********************************************/
        #endregion
        /***********************************************/
    }
}
