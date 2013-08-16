using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vortex.Interface.World.Blocks;
using Vortex.Interface;
using Vortex.Net.Messages;
using Vortex.Interface.Net;

namespace Vortex.Server.World.Blocks
{
    public class BlockTypeCache: IBlockTypeCache
    {
        private readonly Dictionary<short, BlockProperties> _blocks;
        private readonly IServer _engine;

        public BlockTypeCache(IServer engine, IEnumerable<BlockProperties> blockProperties)
        {
            _blocks = new Dictionary<short, BlockProperties>();
            foreach (var blockdata in blockProperties)
            {
                _blocks[blockdata.GetBlockId()] = blockdata;
            }

            engine.RegisterMessageCallback(typeof(ClientGetBlockTypesMessage), HandleGetBlocksRequest);
        }

        public BlockProperties GetBlockProperties(short id)
        {
            if (_blocks.ContainsKey(id))
                return _blocks[id];
            throw new InvalidOperationException("Block requested was not registered");
        }

        private void HandleGetBlocksRequest(Message msg)
        {
            foreach (var block in _blocks)
            {
                var reply = new ServerBlockDataMessage() { BlockData = block.Value };
                _engine.SendMessageToClient(reply, msg.Sender);
            }
        }
    }
}
