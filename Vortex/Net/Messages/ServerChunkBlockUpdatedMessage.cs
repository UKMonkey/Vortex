using Vortex.Interface.Net;
using Vortex.Interface.World.Chunks;

namespace Vortex.Net.Messages
{
    public class ServerChunkBlockUpdatedMessage : Message
    {
        public ChunkKey Key { get; set; }
        public short X { get; set; }
        public short Y { get; set; }
        public short Z { get; set; }
        public short Value { get; set; }

        protected override void DeserializeImpl(IIncomingMessageStream messageStream)
        {
            Key = messageStream.ReadChunkKey();
            X = messageStream.ReadInt16();
            Y = messageStream.ReadInt16();
            Z = messageStream.ReadInt16();
            Value = messageStream.ReadInt16();
        }

        protected override void SerializeImpl(IOutgoingMessageStream messageStream)
        {
            messageStream.Write(Key);
            messageStream.WriteInt16(X);
            messageStream.WriteInt16(Y);
            messageStream.WriteInt16(Z);
            messageStream.WriteInt16(Value);
        }
    }
}
