using Psy.Core;
using SlimMath;
using Vortex.Interface.Net;

namespace Vortex.Net.Messages
{
    public class ServerPlaySoundDirectionMessage : ServerPlaySoundMessage
    {
        public virtual Vector3 Position { get; set; }

        protected override void DeserializeImpl(IIncomingMessageStream messageStream)
        {
            base.DeserializeImpl(messageStream);
            Position = messageStream.ReadVector();
        }

        protected override void SerializeImpl(IOutgoingMessageStream messageStream)
        {
            base.SerializeImpl(messageStream);
            messageStream.Write(Position);
        }
    }
}
