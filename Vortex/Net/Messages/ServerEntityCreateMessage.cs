using System.Collections.Generic;
using Vortex.Interface.EntityBase;
using Vortex.Interface.Net;
using Vortex.Interface.World.Chunks;

namespace Vortex.Net.Messages
{
    public class ServerEntityCreateMessage : Message
    {
        public uint FrameNumber { get; set; }
        public List<Entity> Entities { get; set; }
        public ChunkKey Area { get; set; }

        protected override void DeserializeImpl(IIncomingMessageStream messageStream)
        {
            Entities = messageStream.ReadEntities();
            Area = messageStream.ReadChunkKey();
            FrameNumber = messageStream.ReadUInt32();
        }

        protected override void SerializeImpl(IOutgoingMessageStream messageStream)
        {
            messageStream.Write(Entities);
            messageStream.Write(Area);
            messageStream.WriteUInt32(FrameNumber);
        }
    }
}
