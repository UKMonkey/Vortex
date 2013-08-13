using Vortex.Interface.Net;

namespace Vortex.Net.Messages
{
    public class ServerClientLeaveMessage : Message
    {
        public ushort ClientId { get; set; }


        protected override void DeserializeImpl(IIncomingMessageStream messageStream)
        {
            ClientId = messageStream.ReadUint16();
        }

        protected override void SerializeImpl(IOutgoingMessageStream messageStream)
        {
            messageStream.WriteUInt16(ClientId);
        }
    }
}
