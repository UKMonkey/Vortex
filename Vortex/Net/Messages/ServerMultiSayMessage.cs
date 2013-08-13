using System.Collections.Generic;
using Vortex.Interface.Net;

namespace Vortex.Net.Messages
{
    public class ServerMultiSayMessage : Message
    {
        public ushort ClientId { get; set; }
        public List<string> Text { get; set; }

        public ServerMultiSayMessage()
        {
            ClientId = 0;
            Text = new List<string>();
        }

        public ServerMultiSayMessage(string message)
        {
            ClientId = 0;
            Text = new List<string>(1) { message };
        }

        protected override void DeserializeImpl(IIncomingMessageStream messageStream)
        {
            ClientId = messageStream.ReadUint16();
            Text = messageStream.ReadStringList();
        }

        protected override void SerializeImpl(IOutgoingMessageStream messageStream)
        {
            messageStream.WriteUInt16(ClientId);
            messageStream.Write(Text);
        }
    }
}
