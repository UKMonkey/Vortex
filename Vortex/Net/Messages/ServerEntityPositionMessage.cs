using System.Collections.Generic;
using SlimMath;
using Vortex.Interface.Net;

namespace Vortex.Net.Messages
{
    public class ServerEntityPositionMessage : Message
    {
        public uint FrameNumber { get; set; }
        public int EntityId { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 MovementVector { get; set; }
        public float Rotation { get; set; }

        protected override void DeserializeImpl(IIncomingMessageStream messageStream)
        {
            EntityId = messageStream.ReadEntityId();
            Position = messageStream.ReadVector();
            Rotation = messageStream.ReadFloat();
            MovementVector = messageStream.ReadVector();
            FrameNumber = messageStream.ReadUInt32();
        }

        protected override void SerializeImpl(IOutgoingMessageStream messageStream)
        {
            messageStream.WriteEntityId(EntityId);
            messageStream.Write(Position);
            messageStream.WriteFloat(Rotation);
            messageStream.Write(MovementVector);
            messageStream.WriteUInt32(FrameNumber);
        }

        public override IEnumerable<int> EntityIds()
        {
            return new List<int>{EntityId};
        }
    }
}
