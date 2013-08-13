using Vortex.Interface.Net;

namespace Vortex.Net.Messages
{
    public class ServerWeatherChangeMessage : Message
    {
        public bool IsRaining { get; set; }

        protected override void DeserializeImpl(IIncomingMessageStream messageStream)
        {
            IsRaining = messageStream.ReadBoolean();
        }

        protected override void SerializeImpl(IOutgoingMessageStream messageStream)
        {
            messageStream.WriteBool(IsRaining);
        }
    }
}