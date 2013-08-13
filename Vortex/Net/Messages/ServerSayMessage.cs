using System;
using Vortex.Interface.Net;

namespace Vortex.Net.Messages
{
    public class ServerSayMessage : Message
    {
        public ushort ClientId { get; set; }
        public String Text { get; set; }

        protected override void DeserializeImpl(IIncomingMessageStream messageStream)
        {
            ClientId = messageStream.ReadUint16();
            Text = messageStream.ReadString();
        }

        protected override void SerializeImpl(IOutgoingMessageStream messageStream)
        {
            messageStream.WriteUInt16(ClientId);
            messageStream.Write(Text);
        }
    }
}
