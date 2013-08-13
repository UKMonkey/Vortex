using System.Collections.Generic;
using Vortex.Interface.Net;

namespace Vortex.Net.Messages
{
    public class ServerEntityDestroyMessage : Message
    {
        public List<int> Entities;


        protected override void DeserializeImpl(IIncomingMessageStream messageStream)
        {
            Entities = messageStream.ReadEntityIds();
        }

        protected override void SerializeImpl(IOutgoingMessageStream messageStream)
        {
            messageStream.Write(Entities);
        }

        public override IEnumerable<int> EntityIds()
        {
            return Entities;
        }
    }
}
