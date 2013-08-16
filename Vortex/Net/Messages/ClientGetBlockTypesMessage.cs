using Vortex.Interface.Net;
using Vortex.Interface.World.Chunks;

namespace Vortex.Net.Messages
{
    public class ClientGetBlockTypesMessage : Message
    {
        protected override void DeserializeImpl(IIncomingMessageStream messageStream)
        {
        }

        protected override void SerializeImpl(IOutgoingMessageStream messageStream)
        {
        }
    }
}
