using System.Collections.Generic;
using Vortex.Interface.Net;
using Vortex.Interface.World.Chunks;

namespace Vortex.Net.Messages
{
    public class ClientChunkRequestedMessage : Message
    {
        public List<ChunkKey> ChunkKeys { get; set; }

        public ClientChunkRequestedMessage()
        {
            DeliveryMethod = DeliveryMethod.ReliableOrdered;
        }

        protected override void DeserializeImpl(IIncomingMessageStream messageStream)
        {
            ChunkKeys = messageStream.ReadChunkKeys();
        }

        protected override void SerializeImpl(IOutgoingMessageStream messageStream)
        {
            messageStream.Write(ChunkKeys);
        }
    }
}
