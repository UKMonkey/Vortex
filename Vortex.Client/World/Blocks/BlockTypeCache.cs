using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vortex.Interface.World.Blocks;
using Vortex.Interface;
using Vortex.Net.Messages;
using Vortex.Interface.Net;

namespace Vortex.Client.World.Blocks
{
    public class BlockTypeCache : IBlockTypeCache
    {
        private Dictionary<short, BlockProperties> _blocks;
        private bool _dataRequested = false;
        private BlockProperties _default;
        private IClient _engine;

        public BlockTypeCache(IClient engine, BlockProperties defaultBlock)
        {
            _engine = engine;
            
            _engine.RegisterMessageCallback(typeof(ServerBlockDataMessage), HandleNewBlockMessage);
            _default = defaultBlock;
        }

        public BlockProperties GetBlockProperties(short id)
        {
            if (_blocks.ContainsKey(id))
                return _blocks[id];

            if (!_dataRequested)
                _engine.SendMessage(new ClientGetBlockTypesMessage());

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
