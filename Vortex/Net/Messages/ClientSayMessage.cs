using System;
using Vortex.Interface.Net;

namespace Vortex.Net.Messages
{
    public class ClientSayMessage : Message
    {
        public String Text { get; set; }

        protected override void DeserializeImpl(IIncomingMessageStream messageStream)
        {
            Text = messageStream.ReadString();
        }

        protected override void SerializeImpl(IOutgoingMessageStream messageStream)
        {
            messageStream.Write(Text);
        }
    }
}
