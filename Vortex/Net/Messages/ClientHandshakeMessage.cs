using Vortex.Interface.Net;

namespace Vortex.Net.Messages
{
    public class ClientHandshakeMessage : Message
    {
        public string PlayerName { get; set; }

        protected override void DeserializeImpl(IIncomingMessageStream messageStream)
        {
            PlayerName = messageStream.ReadString();
        }

        protected override void SerializeImpl(IOutgoingMessageStream messageStream)
        {
            messageStream.Write(PlayerName);
        }
    }
}
