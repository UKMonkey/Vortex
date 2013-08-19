using Vortex.Interface.Net;

namespace Vortex.Net.Messages
{
    public class ServerHandshakeMessage : Message
    {
        public short ChunkSize { get; set; }

        protected override void DeserializeImpl(IIncomingMessageStream messageStream)
        {
            ChunkSize = messageStream.ReadInt16();
        }

        protected override void SerializeImpl(IOutgoingMessageStream messageStream)
        {
            messageStream.WriteInt16(ChunkSize);
        }
    }
}
