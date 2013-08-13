using Vortex.Interface.Net;

namespace Vortex.Net.Messages
{
    public class ServerCurrentFrameMessage : Message
    {
        public uint CurrentFrameNumber { get; set; }


        protected override void DeserializeImpl(IIncomingMessageStream messageStream)
        {
            CurrentFrameNumber = messageStream.ReadUInt32();
        }

        protected override void SerializeImpl(IOutgoingMessageStream messageStream)
        {
            messageStream.WriteUInt32(CurrentFrameNumber);
        }
    }
}
