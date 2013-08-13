using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Lidgren.Network;
using Psy.Core;
using Psy.Core.Collision;
using Psy.Core.Configuration;
using Psy.Core.Console;
using Psy.Core.Input;
using Psy.Core.Logging;
using Psy.Core.Tasks;
using Psy.Gui;
using Psy.Gui.Loader;
using SlimMath;
using Vortex.BulletTracer;
using Vortex.Client.Audio;
using Vortex.Client.Console;
using Vortex.Client.World.Providers;
using Vortex.Interface;
using Vortex.Interface.Audio;
using Vortex.Interface.EntityBase;
using Vortex.Interface.Net;
using Vortex.Interface.Traits;
using Vortex.Interface.World.Wrapper;
using Vortex.Mod;
using Vortex.Net;
using Vortex.Net.Messages;
using Vortex.PerformanceHud;
using Vortex.Renderer.Camera;
using Vortex.World;
using Vortex.World.Movement;
using Timer = Psy.Core.Timer;
using Vortex.Interface.EntityBase.Properties;

namespace Vortex.Client
{
    sealed class Client : EngineBase<IGameClient>, IClient
    {
        // latency in ms
        public TimeLine LatencyTimeline { get; private set; }
        public int LastKnownLatency 
        { get { return (int)LatencyTimeline.SamplePoints.LastOrDefault(); } }
        

        public uint LastKnownServerFrameNumber { get; private set; }
        public int TargetFrameLagAmount { get { return 10; } }

        public IAudioEngine AudioEngine { get; private set; }
        public XmlLoader GuiLoader { get; private set; }
        public IInputBinder InputBinder { get; private set; }

        public GuiManager Gui
        {
            get { return EngineWindow.Gui; }
        }

        public float ZoomLevel
        {
            get { return EngineWindow.View.ZoomDistance; }
            set { EngineWindow.View.ZoomDistance = value; }
        }

        public readonly EngineWindow EngineWindow;
        public readonly BarChart NetPerformanceBarChart;
        private NetClient _netClient;
        private NetConnection _netConnection;
        private readonly Commands _commands;

        public readonly TimeLine InboundKbpsTimeLine;
        public readonly TimeLine OutboundKbpsTimeLine;
        private readonly Queue<int> _inboundRecvBytes;
        private readonly Queue<int> _outboundRecvBytes;

        private int _prevRecievedBytes;
        private int _prevSentBytes;

        private readonly Dictionary<int, List<Message>> _messagesForEntities;

        private int _myEntityId;
        private NetworkEntityLoader _entityLoader;

        private WorldDataCache _worldDataCache;
        private NetworkTriggerLoader _triggerLoader;
        private NetworkChunkLoader _chunkLoader;

        private readonly int _mainThreadId;
        delegate void MessageDataUpdate();
        private readonly List<MessageDataUpdate> _otherThreadMessages;

        private readonly MessageHandler _messageHandler;

        public Entity Me { get; set; }

        public NetConnectionStatistics ConnectionStatistics
            { get{return _netConnection == null ? null : _netConnection.Statistics;} }

        public IClientConfiguration Configuration { get; private set; }

        internal Client(IClientConfiguration configuration, EngineWindow engineWindow, StartArguments args) : base(args)
        {
            LatencyTimeline = new TimeLine(320, "Latency", 300);
            Configuration = configuration;
            engineWindow.FormClosed += Disconnect;

            InboundKbpsTimeLine = new TimeLine(320, "Net In", 30 * 1024);
            OutboundKbpsTimeLine = new TimeLine(320, "Net Out", 8 * 1024);
            NetPerformanceBarChart = new BarChart();

            _inboundRecvBytes = new Queue<int>();
            _outboundRecvBytes = new Queue<int>();

            AudioEngine = AudioEngineFactory.Create();
            _messagesForEntities = new Dictionary<int, List<Message>>();

            StaticTaskQueue.TaskQueue.CreateRepeatingTask("Client.NetUpdateChart", NetStatChartUpdate, 1000);
            StaticTaskQueue.TaskQueue.CreateRepeatingTask("Client.Update", Update, 16);
            StaticTaskQueue.TaskQueue.CreateRepeatingTask("Client.RemoveExpiredMessages", RemoveExpiredMessages, 500);

            EngineWindow = engineWindow;
            GuiLoader = new XmlLoader(EngineWindow.Gui);

            _commands = new Commands(this);
            _commands.BindConsoleCommands();

            InputBinder = new InputBinder();

            _messageHandler = new MessageHandler(this);
            _messageHandler.RegisterHandlers();
            RegisterMessageCallback(typeof(ServerCurrentFrameMessage), HandleServerFrameMessage);

            _mainThreadId = Thread.CurrentThread.ManagedThreadId;
            _otherThreadMessages = new List<MessageDataUpdate>();
        }

        // tries to get the client to a suitable frame
        // unlikely to get there immediately, but with time will catch up
        private void HandleServerFrameMessage(Message msg)
        {
            var message = (ServerCurrentFrameMessage)msg;

            var modificationFrameCount = (uint)Math.Round((float)LastKnownLatency / (float)UpdateWorldFrequency);
            LastKnownServerFrameNumber = message.CurrentFrameNumber - modificationFrameCount;
            
            FastForwardToFrame(message.CurrentFrameNumber-5);
        }

        protected override void AttachMod()
        {
            var gameDllName = StaticConfigurationManager.ConfigurationManager.GetString("ModClientDLL");
            var module = ModLoader.LoadModule<IGameClient>(gameDllName, EngineInitArgs);

            if (module == null)
            {
                throw new EngineException(String.Format("Failed to load mod `{0}`", gameDllName));
            }

            module.OnAttach(this);
            Game = module;
            Game.OnBegin();
        }

        private void Update()
        {
            if (Me != null)
            {
                EngineWindow.View.ViewAngle = Me.GetViewAngleRange();
                EngineWindow.View.MinViewRange = Me.GetMeleeViewRange();
                EngineWindow.View.ViewRange = Me.GetViewRange();
                EngineWindow.View.ViewDirection = Me.GetRotation();
            }
            EngineWindow.Gui.Update();
            UpdateNetworkCharts();
        }

        private void UpdateNetworkCharts()
        {
            if (_netConnection == null)
                return;

            if (_inboundRecvBytes.Count > 1)
            {
                InboundKbpsTimeLine.AddSample(_inboundRecvBytes.Average());
            }

            if (_outboundRecvBytes.Count > 1)
            {
                OutboundKbpsTimeLine.AddSample(_outboundRecvBytes.Average());
            }

            LatencyTimeline.AddSample(LatencyTimeline.LastValue);
        }

        /// <summary>
        /// Called every second.
        /// </summary>
        private void NetStatChartUpdate()
        {
            if (_netConnection == null)
                return;

            var bytesRecievedInThisTimeSlice = _netConnection.Statistics.ReceivedBytes - _prevRecievedBytes;
            var bytesSentInThisTimeSlice = _netConnection.Statistics.SentBytes - _prevSentBytes;

            _inboundRecvBytes.Enqueue(bytesRecievedInThisTimeSlice);
            _outboundRecvBytes.Enqueue(bytesSentInThisTimeSlice);

            _prevRecievedBytes = _netConnection.Statistics.ReceivedBytes;
            _prevSentBytes = _netConnection.Statistics.SentBytes;

            if (_inboundRecvBytes.Count > 10)
                _inboundRecvBytes.Dequeue();
            if (_outboundRecvBytes.Count > 10)
                _outboundRecvBytes.Dequeue();

            // sums up packet sizes over a period, Tick calls it output the latest stats.
            NetPerformanceBarChart.Tick();
        }

        public override IConsole Console
        {
            get { return StaticConsole.Console; }
        }


        private void UpdateMessageStats(string direction, String msgType, int length)
        {
            NetPerformanceBarChart.AddSample(direction, msgType,
                                                 new NetworkPerformance { BarValue = length });
        }

        public override void SendMessage(Message msg)
        {
            var message = SerializeMessage(msg);
            var threadId = Thread.CurrentThread.ManagedThreadId;

            if (_netConnection != null)
                _netConnection.SendMessage(message, DeliveryMethodMapper.Map(msg.DeliveryMethod), msg.Channel);

            if (_mainThreadId == threadId)
            {
                UpdateMessageStats("Outgoing", msg.GetType().Name, message.LengthBytes);
            }
            else
            {
                lock (this)
                {
                    MessageDataUpdate toAdd = () => UpdateMessageStats("Outgoing", msg.GetType().Name, message.LengthBytes);
                    _otherThreadMessages.Add(toAdd);
                }
            }
        }

        public void ShowSplashImage(string filename)
        {
            EngineWindow.SplashScreen.SetImage(filename);
            EngineWindow.SplashScreen.Visible = true;
        }

        public void HideSplashImage()
        {
            EngineWindow.SplashScreen.Visible = false;
        }

        public override void LoadMapCompleted()
        {
            EngineWindow.View.UseWorld(World);
            DataCache.OnEntitiesDeleted += OnEntitiesDestroyed;
            DataCache.OnEntitiesLoaded += OnEntitiesCreated;
        }

        public override void ConsoleText(string text, Color4 colour)
        {
            StaticConsole.Console.AddLine(text, colour);
        }

        public override void ConsoleText(string text)
        {
            StaticConsole.Console.AddLine(text);
        }

        public void BloodSpray(Vector3 position, float direction)
        {
            EngineWindow.View.BloodRenderer.AddShot(position, direction);
        }

        public void SetCameraTarget(Entity entity)
        {
            var camera = EngineWindow.View.CreateEntityFollowCamera(entity);
            EngineWindow.View.CameraPosition = camera;
        }

        protected override NetOutgoingMessage CreateNetworkMessage()
        {
            return _netClient.CreateMessage();
        }

        protected override void OnMessage(string msgType, int byteLength)
        {
            UpdateMessageStats("Incomming", msgType, byteLength);
        }

#region Entities
        private void OnEntitiesDestroyed(List<Entity> entities)
        {
            foreach (var entity in entities)
                OnEntityDestroyed(entity);
        }

        private void OnEntityDestroyed(Entity entity)
        {
            _messagesForEntities.Remove(entity.EntityId);
        }

        private void OnEntitiesCreated(List<Entity> entities)
        {
            foreach (var entity in entities)
            {
                List<Message> messagesToProcess;
                if (!_messagesForEntities.TryGetValue(entity.EntityId, out messagesToProcess))
                    continue;

                _messagesForEntities.Remove(entity.EntityId);
                HandleMessage(messagesToProcess);
            }
        }
#endregion

        public Vector3 WorldMousePosition
        {
            get
            {
                return
                    BasicCamera.ScreenToWorldCoordinate(
                            EngineWindow.GraphicsContext,
                            EngineWindow.MousePosition.X, EngineWindow.MousePosition.Y);
            }
        }

        public Vector3 CameraPosition
        {
            get
            {
                // todo: fix. doesn't actually give the camera vector in the world.
                return EngineWindow.View.Camera.Vector;
            }
        }

        public Ray CameraMouseRay
        {
            get
            {
                return BasicCamera.ScreenToWorldRay(
                    EngineWindow.GraphicsContext, 
                    EngineWindow.MousePosition.X,
                    EngineWindow.MousePosition.Y);
            }
        }

        public int MyEntityId
        {
            get { return _myEntityId; }
            set { _myEntityId = value; }
        }

        private void WriteHailMessageFields(NetOutgoingMessage hailMessage)
        {
            hailMessage.Write(Version);
            hailMessage.Write(ModName);
            hailMessage.Write(Game.Version);
        }

        public void ConnectToServer(string hostname, int port)
        {
            if (NetworkStatus != NetworkStatus.Disconnected 
                && NetworkStatus != NetworkStatus.Errored 
                && NetworkStatus != NetworkStatus.Rejected)
            {
                Disconnect();
            }

            var peerConfiguration = new NetPeerConfiguration(StaticConfigurationManager.ConfigurationManager.GetString("Net.AppIdent"))
                                        {
                                            ConnectionTimeout = 500
                                        };
            _netClient = new NetClient(peerConfiguration);
            _netClient.Configuration.EnableMessageType(NetIncomingMessageType.ConnectionLatencyUpdated);
            _netClient.Start();

            var hailMessage = _netClient.CreateMessage();
            WriteHailMessageFields(hailMessage);

            NetworkStatus = NetworkStatus.Connecting;
            var hostData = NetUtility.Resolve(hostname);

            if (hostData == null)
                throw new Exception("Unable to locate " + hostname);

            _netConnection = _netClient.Connect(hostname, port, hailMessage);
        }

        public void Disconnect()
        {
            if (_netClient != null)
                _netClient.Disconnect("Player requested disconnect");

            _netConnection = null;

            if (_entityLoader != null) _entityLoader.Dispose();
            _entityLoader = null;

            if (_triggerLoader != null) _triggerLoader.Dispose();
            _triggerLoader = null;

            if (_chunkLoader != null) _chunkLoader.Dispose();
            _chunkLoader = null;

            if (_worldDataCache != null) _worldDataCache.Dispose();
            _worldDataCache = null;

            NetworkStatus = NetworkStatus.Disconnected;
            UnloadMap();
            _messagesForEntities.Clear();

            EngineWindow.View.UnloadWorld();

            NetPerformanceBarChart.Clear();
        }

        public override void BroadcastSay(string messageString)
        {
            var msg = new ClientSayMessage {Text = messageString};
            SendMessage(msg);
        }

        public override void SendRconCommand(string command, string password)
        {
            var msg = new ClientConsoleCommandMessage {Command = command, Password = password};
            SendMessage(msg);
        }

        public void FireBullet(Vector3 @from, Vector3 to, Color4 colour)
        {
            World.Bullets.Add(new Bullet(@from, to) { Colour = colour });
        }

        public void RegisterViewSystemForEntity(Entity item, EntityTester entityTester,
            EntityHandler onVisible,
            EntityHandler onHidden)
        {
            World.RegisterEntityViewSystem(item, entityTester, onVisible, onHidden);
        }

        public void SetEntityViewCollector(EntityCollection entityCollection)
        {
            EngineWindow.View.EntityCollectionSystem = entityCollection;
        }

        private void RemoveExpiredMessages()
        {
            var now = Timer.GetTime();
            foreach (var data in _messagesForEntities.Select(data => data.Value))
            {
                var expired = data.Where(msg => msg.HasExpired(now)).ToList();
                foreach (var msg in expired)
                {
                    data.Remove(msg);
                }
            }
        }

        private bool StoreMessageIfRequired(Message decodedMsg)
        {
            var shouldStore = false;
            var expired = decodedMsg.HasExpired();

            if (decodedMsg.EntityIds() == null)
                return false;
            
            int missingEntity = 0;
                
            foreach (var id in decodedMsg.EntityIds())
            {
                if (World.GetEntity(id) != null)
                    continue;

                missingEntity = id;
                shouldStore = true;
                break;
            }

            if (!expired && shouldStore && !(_messagesForEntities.ContainsKey(missingEntity)))
            {
                _messagesForEntities.Add(missingEntity, new List<Message>());
            }
            if (!expired && shouldStore)
            {
                _messagesForEntities[missingEntity].Add(decodedMsg);
            }

            return shouldStore;
        }

        private void HandleMessage(IEnumerable<Message> messages)
        {
            foreach (var msg in messages)
            {
                HandleMessage(msg);
            }
        }

        protected override void HandleMessage(Message decodedMsg)
        {
            // require all entity ids
            // a messages with multiple requirements should only be added to a single queue ...
            // when a message's requirement is met, 
            var stored = StoreMessageIfRequired(decodedMsg);
            if (!stored)
            {
                base.HandleMessage(decodedMsg);
            }
        }

        private const int MaxMessagesToProcess = 20;
        protected override void UpdateNetworking()
        {
            NetIncomingMessage msg;

            lock(this)
            {
                foreach (var update in _otherThreadMessages)
                {
                    update();
                }

                _otherThreadMessages.Clear();
            }

            if (_netClient == null)
                return;

            var messagesProcessed = 0;
            while (messagesProcessed < MaxMessagesToProcess && (msg = _netClient.ReadMessage()) != null)
            {
                ++messagesProcessed;
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.StatusChanged:
                        var status = (NetConnectionStatus)msg.ReadByte();

                        switch (status)
                        {
                            case NetConnectionStatus.Connected:
                                Game.OnNetworkStatusChanged(NetworkStatus.Connected);
                                NetworkStatus = NetworkStatus.Connected;
                                break;
                            case NetConnectionStatus.Disconnected:
                                Logger.Write("Disconnect message recieved");
                                Game.OnNetworkStatusChanged(NetworkStatus.Disconnected);
                                Disconnect();
                                break;
                            case NetConnectionStatus.Disconnecting:
                                Logger.Write("Disconnecting from the server");
                                break;
                            default:
                                throw new Exception("Unknown network connection status type");
                        }

                        break;

                    case NetIncomingMessageType.Data:
                        OnMessage(msg);
                        break;

                    case NetIncomingMessageType.Error:
                        throw new Exception("Lidgren network error");

                    case NetIncomingMessageType.ErrorMessage:
                        Logger.Write("ERROR MESSAGE:", LoggerLevel.Error);
                        Logger.Write(msg.ReadString(), LoggerLevel.Error);
                        throw new Exception("Lidgren network error message");

                    case NetIncomingMessageType.DebugMessage:
                        Logger.Write("DEBUG MESSAGE:", LoggerLevel.Trace);
                        Logger.Write(msg.ReadString(), LoggerLevel.Trace);
                        break;
                    
                    case NetIncomingMessageType.ConnectionLatencyUpdated:
                        LatencyTimeline.AddSample(_netConnection.AverageRoundtripTime * 1000f);
                        break;

                    default:
                        Logger.Write(string.Format("Unhandled event type {0}", msg.MessageType), LoggerLevel.Warning);
                        break;
                }
                _netClient.Recycle(msg);
            }
        }

        internal void AttemptEntitiesChange(List<Entity> entities)
        {
            var entity = entities.LastOrDefault(item => item.EntityId == _myEntityId);
            
            if (entity == null)
                return;

            DataCache.OnEntitiesLoaded -= AttemptEntitiesChange;
            DataCache.OnEntitiesUpdated -= AttemptEntitiesChange;

            if (Me != null && Me.EntityId == entity.EntityId)
                return;

            Game.OnFocusChange(entity);
            entity.OnPropertyChanged += MyPropertyChanged;
        }

        public void OnMapMouseMove(Vector3 viewCoords)
        {
            Game.OnWorldMouseMove(viewCoords);
        }

        protected override WorldDataCache GetCachingStrategy()
        {
            if (_worldDataCache != null)
                return _worldDataCache;

            _entityLoader = new NetworkEntityLoader(this);
            _triggerLoader = new NetworkTriggerLoader(this);
            _chunkLoader = new NetworkChunkLoader(this);

            _worldDataCache = new WorldDataCache(
                                    new SimpleWorldProviderWrapper(_chunkLoader, _entityLoader, _triggerLoader),
                                    new WorldSaverWrapper(), 
                                    GetObservedChunkKeys);

            return _worldDataCache;
        }

        protected override IMovementHandler GetMovementHandler()
        {
            return new GroupHandler(new List<IMovementHandler> { new VectorHandler(), new RotationHandler() });
        }

        public override CollisionResult TraceRay(Vector3 source, Vector3 target, Func<Entity, bool> filter)
        {
            var result = base.TraceRay(source, target, filter);

            EngineWindow.View.RayTraceRenderer.AddRay(source, target, new Color4(0.3f, 0.3f, 0.3f, 0.3f));
            EngineWindow.View.RayTraceRenderer.AddRay(source, result.CollisionPoint);

            return result;
        }

        public override CollisionResult TraceRay(Vector3 from, float angle, Func<Entity, bool> filter)
        {
            var result = base.TraceRay(@from, angle, filter);

            var vec = DirectionUtil.CalculateVector(angle);
            EngineWindow.View.RayTraceRenderer.AddRay(from, from + vec, new Color4(0.3f, 0.3f, 0.3f, 0.3f));
            EngineWindow.View.RayTraceRenderer.AddRay(@from, result.CollisionPoint);

            return result;
        }

        /**
         * Don't call this unless you really want to smash the cache ....
         */
        public IEnumerable<Entity> FullEntityList()
        {
            return _worldDataCache.GetEntities();
        }

        public void PlaySound(int soundId, Vector3 source, byte soundChannel)
        {
            var audioFilename = AudioLookup.GetSoundFile(soundId);
            AudioEngine.Play(audioFilename, source, soundChannel);
        }

        public void PlaySound(int soundId, byte soundChannel)
        {
            var audioFilename = AudioLookup.GetSoundFile(soundId);
            AudioEngine.Play(audioFilename, soundChannel);
        }

        public void OnFocusChange(int entityId)
        {
            MyEntityId = entityId;

            var entities = World.GetObservedEntities();
            var entity = entities.FirstOrDefault(e => e.EntityId == entityId);
            if (entity == null)
            {
                DataCache.OnEntitiesLoaded += AttemptEntitiesChange;
                DataCache.OnEntitiesUpdated += AttemptEntitiesChange;
                return;
            }

            Game.OnFocusChange(entity);
            entity.OnPropertyChanged += MyPropertyChanged;
        }

        private void MyPropertyChanged(Entity entity, Trait property)
        {
            if (property.PropertyId == (short)EntityPropertyEnum.Position ||
                property.PropertyId == (short)EntityPropertyEnum.Rotation)
            {
                AudioEngine.UpdateListenerPosition(entity);
            }
        }

        public void UpdateRemotePlayerList(List<RemotePlayer> remotePlayers)
        {
            RemotePlayers.Clear();
            foreach (var player in remotePlayers)
                RemotePlayers.AddRemotePlayer(player);

            Game.OnConnectedClientListRecieved();
        }

        public void OnClientLeave(ushort clientId)
        {
            var client = RemotePlayers.RemoveRemotePlayer(clientId);
            if (client == null)
                return;
            Game.OnClientLeave(client);
        }

        public void OnClientJoin(ushort clientId, string playerName)
        {
            var client = new RemotePlayer(clientId, playerName);
            RemotePlayers.AddRemotePlayer(client);
            Game.OnClientJoin(client);
        }
    }
}
