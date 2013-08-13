using Vortex.Interface.Net;

namespace Vortex.Net.Messages
{
    public class ServerRejectMessage: Message
    {
        public RejectionReasonEnum Reason { get; set; }

        protected override void DeserializeImpl(IIncomingMessageStream messageStream)
        {
            Reason = (RejectionReasonEnum)messageStream.ReadByte();
        }

        protected override void SerializeImpl(IOutgoingMessageStream messageStream)
        {
            messageStream.WriteByte((byte)Reason);
        }
    }
}
