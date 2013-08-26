using System;
using System.Collections.Generic;
using Vortex.Interface.World.Blocks;
using Vortex.Interface;
using Vortex.Net.Messages;
using Vortex.Interface.Net;

namespace Vortex.Client.World.Blocks
{
    public class BlockTypeCache : IBlockTypeCache
    {
        private readonly Dictionary<ushort, BlockProperties> _blocks;
        private readonly BlockProperties _default;
        private readonly IClient _engine;

        public BlockTypeCache(IClient engine, BlockProperties defaultBlock)
        {
            _engine = engine;
            
            _engine.RegisterMessageCallback(typeof(ServerBlockDataMessage), HandleNewBlockMessage);
            _default = defaultBlock;
            _blocks = new Dictionary<ushort, BlockProperties>();
            _engine.SendMessage(new ClientGetBlockTypesMessage());
        }

        public void RegisterProperties(BlockProperties props)
        {
            // client isn't allowed to register its own properties
            // it must get them from the server
            throw new NotImplementedException();
        }

        public BlockProperties GetBlockProperties(ushort id)
        {
            if (_blocks.ContainsKey(id))
                return _blocks[id];

            return _default;
        }

        private void HandleNewBlockMessage(Message msg)
        {
            var message = (ServerBlockDataMessage)msg;
            var blockData = message.BlockData;
            var id = blockData.GetBlockId();

            _blocks[id] = blockData;
        }
    }
}
