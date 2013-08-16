using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vortex.Interface.Net;
using Vortex.Interface.World.Blocks;


namespace Vortex.Net.Messages
{
    public class ServerBlockDataMessage : Message
    {
        public BlockProperties BlockData { get; set; }

        protected override void DeserializeImpl(IIncomingMessageStream messageStream)
        {
            BlockData = messageStream.Read<BlockProperty, BlockProperties>(messageStream);
        }

        protected override void SerializeImpl(IOutgoingMessageStream messageStream)
        {
            messageStream.Write<BlockProperty>(BlockData);
        }
    }
}
