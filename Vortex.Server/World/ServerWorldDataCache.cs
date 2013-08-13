using System.Collections.Generic;
using System.Linq;
using Vortex.Interface.EntityBase.Behaviours;
using Psy.Core.Logging;
using Vortex.Interface;
using Vortex.Interface.EntityBase;
using Vortex.Interface.EntityBase.Properties;
using Vortex.Interface.Net;
using Vortex.Interface.World.Chunks;
using Vortex.Interface.World.Triggers;
using Vortex.Interface.World.Wrapper;
using Vortex.Net.Messages;
using Vortex.World;

namespace Vortex.Server.World
{
    public class ServerWorldDataCache : WorldDataCache
    {
        private readonly Dictionary<RemotePlayer, HashSet<ChunkKey>> _requestedChunks;
        private readonly Dictionary<RemotePlayer, HashSet<ChunkKey>> _requestedTriggers;
        private readonly IServer _engine;


        public ServerWorldDataCache(IServer engine, 
            IWorldProvider worldProvider,
            IWorldSaver worldSaver,
            ObservedChunkKeyDelegate observedChunkKeys)
            : base(worldProvider, worldSaver, observedChunkKeys)
        {
            _engine = engine;

            engine.RegisterMessageCallback(typeof(ClientChunkRequestedMessage), OnChunkRequest);
            engine.RegisterMessageCallback(typeof(TriggerRequestedMessage), OnTriggerRequest);
            engine.RegisterMessageCallback(typeof(ClientEntityRequestedMessage), OnEntityRequest);

            _requestedChunks = new Dictionary<RemotePlayer, HashSet<ChunkKey>>();
            _requestedTriggers = new Dictionary<RemotePlayer, HashSet<ChunkKey>>();

            OnChunksLoaded += ProcessForClients;
            OnChunksUpdated += SendChunks;

            OnTriggerLoaded += ProcessForClients;
            OnTriggerUpdated += SendTriggers;

            OnEntitiesLoaded += ProcessForClients;
            OnEntitiesUpdated += SendEntities;
            OnEntitiesDeleted += SendDeleteEntities;
        }

        /** We need to call spawn once on the server / entity
         *  This once needs to be persistant - ie loaded entities
         *  need spawn to not be called
         */ 
        protected override void HandleNewEntities(List<Entity> entities)
        {
            foreach (var item in entities)
                item.OnSpawn();
        }

        /** An incomming message requesting a collection of chunks ...
         */
        private void OnChunkRequest(Message msg)
        {
            var message = (ClientChunkRequestedMessage) msg;

            lock (_requestedChunks)
            {
                var chunksToGet = message.ChunkKeys;
                var requester = msg.Sender;

                Logger.Write("Requested chunks:", LoggerLevel.Info);
                foreach (var item in chunksToGet)
                {
                    Logger.Write(string.Format("{0} requested chunk {1},{2}", msg.Sender.PlayerName, item.X, item.Y), LoggerLevel.Info);
                }

                // get any chunks that we already have
                var available = GetChunks(chunksToGet);
                if (available.Count != 0)
                {
                    SendChunks(requester, available);
                }

                // register any not sent out so that once loaded we know what to do.
                if (available.Count == chunksToGet.Count)
                    return;
                if (!_requestedChunks.ContainsKey(requester))
                {
                    _requestedChunks.Add(requester, new HashSet<ChunkKey>());
                }
                var toHandleLater = _requestedChunks[requester];

                foreach (var key in available.Select(item => item.Key).Where(key => !chunksToGet.Contains(key)))
                {
                    toHandleLater.Add(key);
                }
            }
        }

        /** An incomming message requesting triggers for a chunk area
         */
        private void OnTriggerRequest(Message msg)
        {
            var message = (TriggerRequestedMessage) msg;
            var chunkToGet = message.ChunkKey;
            var requester = message.Sender;

            var available = GetTriggers(chunkToGet);
            if (available.Count != 0)
            {
                SendTriggers(requester, chunkToGet, available);
                return;
            }
            if (!_requestedTriggers.ContainsKey(requester))
            {
                _requestedTriggers.Add(requester, new HashSet<ChunkKey>());
            }
            _requestedTriggers[requester].Add(chunkToGet);
        }

        /** Check what clients have requested anything in the list, and send it to them.
         */
        private void ProcessForClients(List<Chunk> chunks)
        {
            lock (_requestedChunks)
            {
                SendChunks(chunks);
            }
        }

        /**
         */
        private void ProcessForClients(ChunkKey key, List<ITrigger> triggers)
        {
            foreach(var entry in _requestedTriggers)
            {
                if (entry.Value.Count == 0)
                    continue;

                var toSend = triggers.Where(trigger => entry.Value.Contains(trigger.UniqueKey.ChunkLocation)).ToList();
                SendTriggers(key, toSend);
            }
        }

        /** Send a load of chunks to all the clients ...
         */
        private void SendChunks(List<Chunk> chunks)
        {
            if (chunks.Count == 0)
                return;

            Logger.Write(string.Format("Transmitting {0} chunks:", chunks.Count), LoggerLevel.Trace);
            foreach (var item in chunks)
            {
                Logger.Write(item.Key.X + ", " + item.Key.Y + ":", LoggerLevel.Trace);
            }

            foreach (var item in chunks)
            {
                var msg = new ServerChunkUpdatedMessage {Chunk = item};

                _engine.SendMessage(msg);
            }
        }

        /** Send a load of chunks to one specific client
         */
        private void SendChunks(RemotePlayer target, List<Chunk> chunks)
        {
            if (chunks.Count == 0)
                return;

            Logger.Write(string.Format("Transmitting {0} chunks:", chunks.Count), LoggerLevel.Trace);
            foreach (var item in chunks)
            {
                Logger.Write(item.Key.X + ", " + item.Key.Y + ":", LoggerLevel.Trace);
            }

            foreach (var item in chunks)
            {
                var msg = new ServerChunkUpdatedMessage {Chunk = item};
                _engine.SendMessageToClient(msg, target);
            }
        }

        /** Send a load of triggers to all the clients ...
         */
        private void SendTriggers(ChunkKey key, List<ITrigger> triggers)
        {
            if (triggers.Count == 0)
                return;

            //TODO
            var msg = new TriggerUpdatedMessage();
        }

        /** Send a load of triggers to one specific client
         */
        private void SendTriggers(RemotePlayer target, ChunkKey key, List<ITrigger> triggers)
        {
            if (triggers.Count == 0)
                return;

            //TODO
        }

        /**
         */
        private void OnEntityRequest(Message msg)
        {
            var message = (ClientEntityRequestedMessage) msg;
            var chunk = message.ChunkOfInterest;
            var entitiesToSend = GetEntities(chunk);
            SendEntitiesImpl(entitiesToSend.ToList(), true);
        }

        private void ProcessForClients(List<Entity> entities)
        {
            SendEntitiesImpl(entities, true);
        }

        private void SendEntities(List<Entity> entities)
        {
            SendEntitiesImpl(entities);
        }

        private Message GetPositionMessage(Entity entity, bool force)
        {
            if ((!entity.GetProperty(EntityPropertyEnum.Rotation).IsDirty) &&
                (!entity.GetProperty(EntityPropertyEnum.MovementVector).IsDirty) &&
                (!force) &&
                (entity.GetPlayerId() == null))
                return null;

            var entityPosMsg = new ServerEntityPositionMessage
            {
                EntityId = entity.EntityId,
                Position = entity.GetPosition(),
                Rotation = entity.GetRotation(),
                MovementVector = entity.GetMovementVector(),
                FrameNumber = _engine.CurrentFrameNumber
            };

            entity.GetProperty(EntityPropertyEnum.Rotation).ClearDirtyFlag();
            entity.GetProperty(EntityPropertyEnum.Position).ClearDirtyFlag();
            entity.GetProperty(EntityPropertyEnum.MovementVector).ClearDirtyFlag();

            return entityPosMsg;
        }

        private void GetEntityUpdateMessages(Entity entity, bool force, LinkedList<Message> toSend, Dictionary<ChunkKey, List<Entity>> createsToSend)
        {
            var properties =
                force ? 
                entity.NonDefaultProperties.Where(prop => prop.PropertyId != (short)EntityPropertyEnum.Position) :
                entity.NonDefaultProperties.Where(prop => prop.IsDirty &&
                                       prop.PropertyId != (short)EntityPropertyEnum.Position &&
                                       prop.PropertyId != (short)EntityPropertyEnum.Rotation &&
                                       prop.PropertyId != (short)EntityPropertyEnum.MovementVector);

            var positionMessage = GetPositionMessage(entity, force);
            if (positionMessage != null)
                toSend.AddFirst(positionMessage);

            var sentProperties = properties.ToList();

            if (sentProperties.Count != 0)
            {
                toSend.AddFirst(new ServerEntityPropertyUpdate
                {
                    EntityId = entity.EntityId,
                    Properies = sentProperties
                });

                foreach (var prop in sentProperties)
                    prop.ClearDirtyFlag();
            }

            if (!force)
                return;

            var area = _engine.GetChunkKeyForWorldVector(entity.GetPosition());
            if (!createsToSend.ContainsKey(area))
                createsToSend[area] = new List<Entity>();
            createsToSend[area].Add(entity);
        }

        private void SendEntitiesImpl(List<Entity> entities, bool force = false)
        {
            if (entities.Count == 0)
                return;

            var messageCollection = new Dictionary<Entity, LinkedList<Message>>();
            foreach (var playerEntity in 
                from item in _engine.RemotePlayers.GetPlayers()
                let entityId = item.EntityId
                where entityId != null
                select _engine.GetEntity(entityId.Value))
            {
                messageCollection[playerEntity] = new LinkedList<Message>();
            }

            var entitiesToSend = force ? entities : 
                entities.Where(item => item.IsDirty() && !item.PendingDestruction)
                .ToList();

            if (entitiesToSend.Count == 0)
                return;

            var toSend = new LinkedList<Message>();
            var createMessages = new Dictionary<ChunkKey, List<Entity>>();

            foreach (var entity in entitiesToSend)
            {
                Logger.Write(
                    force
                        ? string.Format("Force sending entity {0}", entity.EntityId)
                        : string.Format("Soft sending entity {0}", entity.EntityId), LoggerLevel.Trace);

                GetEntityUpdateMessages(entity, force, toSend, createMessages);
            }

            foreach (var msg in toSend)
                _engine.SendMessage(msg);

            foreach (var item in createMessages)
            {
                var msg = new ServerEntityCreateMessage
                              {
                                  Entities = item.Value,
                                  Area = item.Key,
                                  FrameNumber = _engine.CurrentFrameNumber
                              };
                _engine.SendMessage(msg);
            };
        }

        private void SendDeleteEntities(List<Entity> entities)
        {
            var msg = new ServerEntityDestroyMessage {Entities = entities.Select(item => item.EntityId).ToList()};
            _engine.SendMessage(msg);
        }
    }
}
