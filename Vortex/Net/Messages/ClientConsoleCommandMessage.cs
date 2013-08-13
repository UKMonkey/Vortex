using System;
using Vortex.Interface.Net;

namespace Vortex.Net.Messages
{
    public class ClientConsoleCommandMessage : Message
    {
        public String Password { get; set; }
        public String Command { get; set; }


        protected override void DeserializeImpl(IIncomingMessageStream messageStream)
        {
            Password = messageStream.ReadString();
            Command = messageStream.ReadString();
        }

        protected override void SerializeImpl(IOutgoingMessageStream messageStream)
        {
            messageStream.Write(Password);
            messageStream.Write(Command);
        }
    }
}
