using System.Collections.Generic;
using Psy.Core.Logging;
using Vortex.Interface;
using Vortex.Interface.EntityBase;
using Vortex.Interface.EntityBase.Properties;
using Vortex.Interface.Net;
using Vortex.Interface.World.Chunks;
using Vortex.Interface.World.Entities;
using Vortex.Net.Messages;

namespace Vortex.Client.World.Providers
{
    public class NetworkEntityLoader : IEntityLoader
    {
        public event EntityChunkKeyCallback OnEntityLoaded;
        public event EntitiesCallback OnEntityUpdated;
        public event EntityIdCallback OnEntityDeleted;

#pragma warning disable 67
        public event ChunkKeyCallback OnEntitiesUnavailable;
        public event EntityChunkKeyCallback OnEntityGenerated;
#pragma warning restore 67

        private readonly IClient _engine;
        private readonly HashSet<short> _clientControledProps = GetControledProperties();

        public NetworkEntityLoader(IClient engine)
        {
            engine.RegisterMessageCallback(typeof(ServerEntityCreateMessage), HandleServerEntityCreateMessage);
            engine.RegisterMessageCallback(typeof(ServerEntityDestroyMessage), HandleServerEntityGoneMessage);
            engine.RegisterMessageCallback(typeof(ServerEntityPropertyUpdate), HandlePropertyUpdatedMessage);
            engine.RegisterMessageCallback(typeof(ServerEntityPositionMessage), HandleEntityPositionMessage);

            _engine = engine;
        }

        public void Dispose()
        {
            _engine.UnregisterMessageCallback(typeof(ServerEntityCreateMessage));
            _engine.UnregisterMessageCallback(typeof(ServerEntityDestroyMessage));
            _engine.UnregisterMessageCallback(typeof(ServerEntityPropertyUpdate));
            _engine.UnregisterMessageCallback(typeof(ServerEntityPositionMessage));
        }

        public void LoadEntities(ChunkKey area)
        {
            var msg = new ClientEntityRequestedMessage {ChunkOfInterest = area};

            Logger.Write(string.Format("Requesting entities in chunks {0}", area), LoggerLevel.Trace);
            _engine.SendMessage(msg);
        }

        public void LoadEntities(List<ChunkKey> area)
        {
            foreach (var item in area)
            {
                LoadEntities(item);
            }
        }

        private void HandleServerEntityCreateMessage(Message msg)
        {
            var message = (ServerEntityCreateMessage) msg;
            var added = message.Entities;

            foreach(var entity in added)
                Logger.Write(string.Format("Entity created: {0}", entity.EntityId), LoggerLevel.Trace);
            OnEntityLoaded(added, message.Area);
        }

        private void HandleServerEntityGoneMessage(Message msg)
        {
            var message = (ServerEntityDestroyMessage) msg;
            var deleted = message.Entities;

            foreach (var id in deleted)
                Logger.Write(string.Format("Got entity {0} deleted from server", id));

            OnEntityDeleted(deleted);
        }

        private void HandlePropertyUpdatedMessage(Message msg)
        {
            var message = (ServerEntityPropertyUpdate)msg;
            var limitProperties = _engine.Me != null &&
                                   message.EntityId == _engine.Me.EntityId;

            var entity = _engine.GetEntity(message.EntityId);

            foreach (var prop in message.Properies)
            {
                Logger.Write(string.Format("Got entity updated from server {0}: {1}", entity.EntityId, prop));

                if (!limitProperties)
                    entity.SetProperty(prop);
                else if (!_clientControledProps.Contains(prop.PropertyId))
                    entity.SetProperty(prop);
            }

            OnEntityUpdated(new List<Entity> { entity });
        }

        private void HandleEntityPositionMessage(Message msg)
        {
            var message = (ServerEntityPositionMessage) msg;

            var entity = _engine.GetEntity(message.EntityId);
            if (entity == _engine.Me)
                return;

            //var framesToProcess = (double)message.FrameNumber - (double)_engine.CurrentFrameNumber - _engine.TargetFrameLagAmount;
            var newPosition = message.Position;// -message.MovementVector * (float)framesToProcess;

            entity.SetPosition(newPosition);
            entity.SetRotation(message.Rotation);
            entity.SetMovementVector(message.MovementVector);

            OnEntityUpdated(new List<Entity>{entity});
        }

        private static HashSet<short> GetControledProperties()
        {
            return new HashSet<short>{(short)EntityPropertyEnum.Position, 
                                      (short)EntityPropertyEnum.MovementVector,
                                      (short)EntityPropertyEnum.Rotation};
        }
    }
}
