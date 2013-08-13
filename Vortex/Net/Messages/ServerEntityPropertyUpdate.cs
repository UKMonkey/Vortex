using System.Collections.Generic;
using Psy.Core.Logging;
using Vortex.Interface.EntityBase.Properties;
using Vortex.Interface.Net;

namespace Vortex.Net.Messages
{
    public class ServerEntityPropertyUpdate : Message
    {
        public int EntityId { get; set; }
        public List<EntityProperty> Properies { get; set; }
        
        public ServerEntityPropertyUpdate()
        {
            //may want to think about the expire time ... but this is probably fine for now
        }

        public override IEnumerable<int> EntityIds()
        {
            return new List<int> {EntityId};
        }

        protected override void DeserializeImpl(IIncomingMessageStream messageStream)
        {
            EntityId = messageStream.ReadEntityId();
            Properies = messageStream.ReadEntityProperties<EntityProperty>();

            foreach (var prop in Properies)
            {
                Logger.Write(string.Format("Got property update for proptype {0}", prop.PropertyId), LoggerLevel.Trace);
            }
        }

        protected override void SerializeImpl(IOutgoingMessageStream messageStream)
        {
            messageStream.WriteEntityId(EntityId);
            messageStream.Write(Properies);

            foreach (var prop in Properies)
            {
                Logger.Write(string.Format("sending property update for proptype {0}", prop.PropertyId), LoggerLevel.Trace);
            }
        }
    }
}
