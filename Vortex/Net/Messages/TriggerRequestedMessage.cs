using System;
using Vortex.Interface.Net;
using Vortex.Interface.World.Chunks;

namespace Vortex.Net.Messages
{
    public class TriggerRequestedMessage : Message
    {
        public ChunkKey ChunkKey { get; set; }


        protected override void DeserializeImpl(IIncomingMessageStream messageStream)
        {
            ChunkKey = messageStream.ReadChunkKey();
        }

        protected override void SerializeImpl(IOutgoingMessageStream messageStream)
        {
            messageStream.Write(ChunkKey);
        }
    }
}
