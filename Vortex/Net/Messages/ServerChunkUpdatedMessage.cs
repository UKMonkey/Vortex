using Vortex.Interface.Net;
using Vortex.Interface.World.Chunks;

namespace Vortex.Net.Messages
{
    public class ServerChunkUpdatedMessage : Message
    {
        public Chunk Chunk { get; set; }

        public ServerChunkUpdatedMessage()
        {
        }

        protected override void DeserializeImpl(IIncomingMessageStream messageStream)
        {
            Chunk = messageStream.ReadChunk();
        }

        protected override void SerializeImpl(IOutgoingMessageStream messageStream)
        {
            messageStream.Write(Chunk);
        }
    }
}
