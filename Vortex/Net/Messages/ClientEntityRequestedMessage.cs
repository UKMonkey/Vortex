using Vortex.Interface.Net;
using Vortex.Interface.World.Chunks;

namespace Vortex.Net.Messages
{
    public class ClientEntityRequestedMessage : Message
    {
        public ChunkKey ChunkOfInterest { get; set; }


        protected override void DeserializeImpl(IIncomingMessageStream messageStream)
        {
            ChunkOfInterest = messageStream.ReadChunkKey();
        }

        protected override void SerializeImpl(IOutgoingMessageStream messageStream)
        {
            messageStream.Write(ChunkOfInterest);
        }
    }
}
