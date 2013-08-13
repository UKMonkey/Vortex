using System.Collections.Generic;
using Vortex.Interface.Net;

namespace Vortex.Net.Messages
{
    public class ServerPlaySoundAtEntityMessage : ServerPlaySoundMessage
    {
        public int EntityId { get; set; }


        public ServerPlaySoundAtEntityMessage()
        {
            ExpiryDelay = 0;
        }

        protected override void DeserializeImpl(IIncomingMessageStream messageStream)
        {
            base.DeserializeImpl(messageStream);
            EntityId = messageStream.ReadEntityId();
        }

        protected override void SerializeImpl(IOutgoingMessageStream messageStream)
        {
            base.SerializeImpl(messageStream);
            messageStream.WriteEntityId(EntityId);
        }

        public override IEnumerable<int> EntityIds()
        {
            return new List<int>{EntityId};
        }
    }
}
