using System;
using System.Collections.Generic;
using System.Linq;
using Psy.Core.Tasks;
using SlimMath;
using Vortex.Interface.EntityBase.Properties;
using Lidgren.Network;
using Psy.Core.Configuration;
using Psy.Core.Logging;
using Vortex.Interface;
using Vortex.Interface.EntityBase;
using Vortex.Interface.Net;
using Vortex.Interface.World.Chunks;
using Vortex.Mod;
using Vortex.Net.Messages;
using Vortex.Server.World;
using Vortex.World;
using Vortex.World.Chunks;
using Vortex.World.Interfaces;
using Vortex.World.Movement;

namespace Vortex.Server
{
    internal class Server : EngineBase<IGameServer>, IServer
    {
        // what frames should we send the current frame msg?
        private readonly int _frameSyncRate;
        public float ObservedSize { get { return Map.MaximumObservableAreaSize; } }

        public IConsoleCommandContext ConsoleCommandContext { get { return _consoleCommandContext; } }
        public ConsoleCommandContext GetConsoleCommandContext()
        {
            return _consoleCommandContext;
        }

        public bool Running { get; set; }

        private readonly NetServer _netServer;
        private ushort _nextPlayerId;

        private int UpdateHandshakeFrequency { get { return 20; } }

        private readonly ClientCollection _partiallyConnectedClients;
        private readonly ClientCollection _fullyConnectedClients;

        private readonly Dictionary<Entity, ICamera> _entityToCameras;
        private readonly MessageHandler _messageHandler;
        private readonly ConsoleCommandContext _consoleCommandContext;

        internal Server(StartArguments args) : base(args)
        {
            _frameSyncRate = 2*UpdateWorldFrequency;
            Running = true;

            _nextPlayerId = 0;
            _entityToCameras = new Dictionary<Entity, ICamera>();
            var netPeerConfiguration = new NetPeerConfiguration(StaticConfigurationManager.ConfigurationManager.GetString("Net.AppIdent"))
            {
                ConnectionTimeout = Convert.ToSingle(StaticConfigurationManager.ConfigurationManager.GetInt("Net.ConnectTimeout")),
                MaximumConnections = Convert.ToInt32(StaticConfigurationManager.ConfigurationManager.GetInt("Net.MaxConnections")),
            };

            _netServer = new NetServer(netPeerConfiguration);

            _partiallyConnectedClients = new ClientCollection(_netServer);
            _fullyConnectedClients = new ClientCollection(_netServer);
            
            _messageHandler = new MessageHandler(this);
            _messageHandler.RegisterHandlers();

            //StaticTaskQueue.TaskQueue.CreateRepeatingTask("Engine: Clearing Cache", ClearCachedData, CacheClearingFrequency);

            RegisterMessageCallback(typeof(ClientHandshakeMessage), HandshakeMessageHandler);
            StaticTaskQueue.TaskQueue.CreateRepeatingTask("ServerEngine.HandleHandshakes", UpdateHandshakes, UpdateHandshakeFrequency);

            _consoleCommandContext = new ConsoleCommandContext();
        }

#region Messages
        private void UpdateHandshakes()
        {
            var msg = new ServerHandshakeMessage();
            var message = SerializeMessage(msg);
            _partiallyConnectedClients.SendMessage(message, msg.DeliveryMethod, msg.Channel);
        }

        private void RejectDueToSameName(RemotePlayer person)
        {
            var msg = new ServerRejectMessage
                {
                    Reason = RejectionReasonEnum.PlayerNameTaken
                };
            SendMessageToClient(msg, person);
            person.Connection.Disconnect("Done");
        }

        private void HandshakeMessageHandler(Message msg)
        {
            var connection = msg.Sender.Connection;
            
            if (!_partiallyConnectedClients.Contains(connection))
                return;
            
            _partiallyConnectedClients.Remove(connection);
            Logger.Write("Client handshake complete");

            var message = (ClientHandshakeMessage) msg;
            msg.Sender.PlayerName = message.PlayerName;
            var client = msg.Sender;
            
            foreach (var otherPlayer in RemotePlayers.GetPlayers())
            {
                if (otherPlayer.PlayerName == message.PlayerName &&
                    otherPlayer.ClientId != message.Sender.ClientId)
                {
                    RejectDueToSameName(msg.Sender);
                    return;
                }
            }

            // inform all other clients that this client has connected.
            var broadcastMsg = new ServerClientJoinMessage(client);
            SendMessage(broadcastMsg);

            // tell this client about all the currently connected clients.
            SendCurrentPlayerList(client);
            _fullyConnectedClients.Add(client.Connection);

            // tell the client what the current frame # is
            var frameMsg = new ServerCurrentFrameMessage {CurrentFrameNumber = CurrentFrameNumber};
            SendMessageToClient(frameMsg, message.Sender);

            Game.OnClientConnected(client);
        }

        protected override void NextFrameStarted()
        {
            if (CurrentFrameNumber % _frameSyncRate != 0)
                return;

            Logger.Write(String.Format("Sending out new frame number {0}", CurrentFrameNumber));
            var frameMsg = new ServerCurrentFrameMessage { CurrentFrameNumber = CurrentFrameNumber };
            SendMessage(frameMsg);
        }

        protected override NetOutgoingMessage CreateNetworkMessage()
        {
            return _netServer.CreateMessage();
        }

        public override void SendMessage(Message msg)
        {
            var message = SerializeMessage(msg);
            _fullyConnectedClients.SendMessage(message, msg.DeliveryMethod, msg.Channel);
        }

        public void SendMessage(Message message, RemotePlayer except)
        {
            var msg = SerializeMessage(message);
            _fullyConnectedClients.SendMessage(msg, except, message.DeliveryMethod, message.Channel);
        }

        public void SendMessageToClient(Message message, RemotePlayer destination)
        {
            var msg = SerializeMessage(message);
            SendMessageToClient(msg, destination);
        }

        private void SendMessageToClient(NetOutgoingMessage message, RemotePlayer destination)
        {
            _netServer.SendMessage(message, destination.Connection, NetDeliveryMethod.ReliableOrdered);
        }

        public override void BroadcastSay(string messageString)
        {
            var message = new ServerMultiSayMessage { Text = new List<string> { messageString } };
            SendMessage(message);
        }
#endregion

#region Entity
        public void SpawnEntity(Entity entity)
        {
            World.AddEntity(entity);
        }

        public Entity GetEntity(RemotePlayer remotePlayer)
        {
            var result = Entities.SingleOrDefault(e => e.GetPlayerId() == remotePlayer.ClientId);
            return result;
        }

        public Entity GetEntity(string playerName)
        {
            return
                Entities
                    .Where(e => e.GetPlayerId() != null)
                    .SingleOrDefault(e =>
                        string.Equals(
                            e.GetPlayer(this).PlayerName, 
                            playerName,
                            StringComparison.InvariantCultureIgnoreCase));
        }

        public override void TrackEntity(Entity thingToTrack)
        {
            var camera = new EntityTrackingCamera(thingToTrack);
            World.GetMap().AddCamera(camera);
            _entityToCameras.Add(thingToTrack, camera);
        }

        public override void StopTrackingEntity(Entity thingToStopTracking)
        {
            if (_entityToCameras.ContainsKey(thingToStopTracking))
            {
                World.GetMap().RemoveCamera(_entityToCameras[thingToStopTracking]);
                _entityToCameras.Remove(thingToStopTracking);
            }
        }
#endregion

#region Game Init Data
        protected override void AttachMod()
        {
            var gameDllName = StaticConfigurationManager.ConfigurationManager.GetString("ModServerDLL");
            var module = ModLoader.LoadModule<IGameServer>(gameDllName, EngineInitArgs);

            if (module == null)
            {
                throw new EngineException(String.Format("Failed to load mod `{0}`", gameDllName));
            }

            module.OnAttach(this);
            Game = module;
            Game.OnBegin();
        }

        protected override WorldDataCache GetCachingStrategy()
        {
            var method = Game.GetWorldHandling();
            var serverWorldDataCache = new ServerWorldDataCache(
                this, method.WorldProvider, method.WorldSaver, GetObservedChunkKeys);

            return serverWorldDataCache;
        }

        protected override IMovementHandler GetMovementHandler()
        {
            return new GroupHandler(new List<IMovementHandler>
                {new VectorHandler(), new RotationHandler()});
        }

        public override void LoadMapCompleted()
        {
        }

        public void Listen(int port)
        {
            _netServer.Configuration.Port = port;
            _netServer.Configuration.SetMessageTypeEnabled(NetIncomingMessageType.ConnectionApproval, true);
            _netServer.Start();
            ChangeNetworkStatus(NetworkStatus.Listening);
        }
#endregion

#region Chunks
        public ChunkKey GetChunkKeyForWorldVector(Vector3 vector)
        {
            return Utils.GetChunkKeyForPosition(vector);
        }

        public void GetChunkVectorForWorldVector(Vector3 worldVector, out ChunkKey key, out Vector3 chunkVector)
        {
            Utils.GetChunkVectorFromWorldVector(worldVector, out key, out chunkVector);
        }

        public void PlaySoundOnEntity(Entity target, byte audioChannel, string soundFilename)
        {
            var msg = new ServerPlaySoundAtEntityMessage
                          {
                              EntityId = target.EntityId,
                              SoundChannel = audioChannel,
                              SoundId = AudioLookup.GetSoundId(soundFilename)
                          };
            SendMessage(msg);
        }

#endregion

#region Console

        public override void ConsoleText(string text, Color4 colour)
        {
            Logger.Write(text);
            System.Console.WriteLine(text);
        }

        public override void ConsoleText(string text)
        {
            Logger.Write(text);
            System.Console.WriteLine(text);
        }

#endregion

        private ushort GetNextPlayerId()
        {
            if (_nextPlayerId == ushort.MaxValue)
            {
                _nextPlayerId = 0;
            }
            _nextPlayerId++;
            return _nextPlayerId;
        }

        private string RejectConnectionIfRequired(NetIncomingMessage message)
        {
            var engineVersion = message.SenderConnection.RemoteHailMessage.ReadString();
            var modName = message.SenderConnection.RemoteHailMessage.ReadString();
            var modVersion = message.SenderConnection.RemoteHailMessage.ReadString();

            if (engineVersion != Version)
            {
                return String.Format(
                    "Incompatible engine version. Client={0}, Server={1}",
                    engineVersion, Version);
            }

            if (modName != ModName)
            {
                return String.Format(
                    "Incompatible mod. Client = {0}, Server = {1}",
                    modName, ModName);
            }

            if (modVersion != Game.Version)
            {
                return String.Format(
                    "Incompatible mod version. Client={0}, Server={1}",
                    modVersion, Game.Version);
            }

            return null;
        }

        /// <summary>
        /// Tell the client to focus the camera on a particular entity.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="entity"></param>
        public void SetClientFocus(RemotePlayer player, Entity entity)
        {
            var msg = new ServerEntityFocusMessage {EntityId = entity.EntityId};
            SendMessageToClient(msg, player);
        }

        private void NetworkStatusChangedToDisconnected(NetIncomingMessage message)
        {
            // remove client from the list.
            var remoteClient = (RemotePlayer)message.SenderConnection.Tag;

            // client never completed the connection.
            if (remoteClient == null)
            {
                Logger.Write("Client disconnected before connection was fully established");
                return;
            }

            RemotePlayers.RemoveRemotePlayer(remoteClient.ClientId);
            ConsoleText(remoteClient.PlayerName + " has disconnected.");

            // inform connected clients that the client has disconnected.
            // inform all other clients that this client has left
            var msg = new ServerClientLeaveMessage {ClientId = remoteClient.ClientId};
            SendMessage(msg);

            // inform the mod the client has disconnected
            if (!_fullyConnectedClients.Contains(remoteClient.Connection))
                return;

            _fullyConnectedClients.Remove(remoteClient.Connection);
            Game.OnClientDisconnected(remoteClient);
        }

        private void HandleStatusChangedMessage(NetIncomingMessage message)
        {
            var status = (NetConnectionStatus)message.ReadByte();

            switch (status)
            {
                case NetConnectionStatus.None:
                    break;
                case NetConnectionStatus.InitiatedConnect:
                    break;
                case NetConnectionStatus.RespondedAwaitingApproval:
                    break;
                case NetConnectionStatus.RespondedConnect:
                    break;
                case NetConnectionStatus.Connected:
                    _partiallyConnectedClients.Add(message.SenderConnection);
                    break;
                case NetConnectionStatus.Disconnecting:
                    break;
                case NetConnectionStatus.Disconnected:
                    NetworkStatusChangedToDisconnected(message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(string.Format("Unknown connection status {0}", status));
            }
        }

        private void ProcessMessage(NetIncomingMessage message)
        {
            switch (message.MessageType)
            {
                case NetIncomingMessageType.Error:
                    break;
                case NetIncomingMessageType.StatusChanged:
                    HandleStatusChangedMessage(message);
                    break;
                case NetIncomingMessageType.UnconnectedData:
                    break;
                case NetIncomingMessageType.ConnectionApproval:
                    var err = RejectConnectionIfRequired(message);
                    if (string.IsNullOrEmpty(err))
                    {
                        message.SenderConnection.Approve();

                        var connection = message.SenderConnection;
                        var client = new RemotePlayer(GetNextPlayerId(), "Unnamed", connection);
                        connection.Tag = client;
                        RemotePlayers.AddRemotePlayer(client);
                    }
                    else
                    {
                        message.SenderConnection.Deny(err);
                        ConsoleText(err);
                    }
                    break;
                case NetIncomingMessageType.Data:
                    OnMessage(message);
                    break;
                case NetIncomingMessageType.Receipt:
                    break;
                case NetIncomingMessageType.DiscoveryRequest:
                    break;
                case NetIncomingMessageType.DiscoveryResponse:
                    break;
                case NetIncomingMessageType.VerboseDebugMessage:
                    break;
                case NetIncomingMessageType.DebugMessage:
                    break;
                case NetIncomingMessageType.WarningMessage:
                    break;
                case NetIncomingMessageType.ErrorMessage:
                    break;
                case NetIncomingMessageType.NatIntroductionSuccess:
                    break;
                case NetIncomingMessageType.ConnectionLatencyUpdated:
                    break;
                default:
                    throw new ArgumentOutOfRangeException("message", "Unknown message type");
            }
        }

        protected override void UpdateNetworking()
        {
            NetIncomingMessage message;
            while ((message = _netServer.ReadMessage()) != null)
            {
                ProcessMessage(message);
                _netServer.Recycle(message);
            }
        }

        private void SendCurrentPlayerList(RemotePlayer player)
        {
            var message = new ServerConnectedClientsMessage();
            message.RemotePlayers = RemotePlayers.GetPlayers().ToList();
            SendMessageToClient(message, player);
        }
    }
}
