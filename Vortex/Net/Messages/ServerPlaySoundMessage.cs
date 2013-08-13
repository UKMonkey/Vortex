using System;
using Vortex.Interface.Net;

namespace Vortex.Net.Messages
{
    public class ServerPlaySoundMessage : Message
    {
        public int SoundId { get; set; }
        public byte SoundChannel { get; set; }

        protected override void DeserializeImpl(IIncomingMessageStream messageStream)
        {
            SoundId = messageStream.ReadInt32();
            SoundChannel = messageStream.ReadByte();
        }

        protected override void SerializeImpl(IOutgoingMessageStream messageStream)
        {
            messageStream.WriteInt32(SoundId);
            messageStream.WriteByte(SoundChannel);
        }
    }
}
