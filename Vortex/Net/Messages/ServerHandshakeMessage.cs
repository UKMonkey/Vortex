using Vortex.Interface.Net;

namespace Vortex.Net.Messages
{
    public class ServerHandshakeMessage : Message
    {
        protected override void DeserializeImpl(IIncomingMessageStream messageStream)
        {
        }

        protected override void SerializeImpl(IOutgoingMessageStream messageStream)
        {
        }
    }
}
