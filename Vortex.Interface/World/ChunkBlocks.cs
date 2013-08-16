namespace Vortex.Interface.World
{
    public delegate void BlockCallback(ChunkBlocks blocks, int x, int y);

    public class ChunkBlocks
    {
        public event BlockCallback BlocksUpdated;

        private const int BlockCount = 16;
        private const int Depth = 1024;
        private readonly short [] _blocks;

        public ChunkBlocks()
        {
            _blocks = new short[BlockCount * BlockCount * Depth];
            for (var i = 0; i < BlockCount; ++i)
            {
                for (var j = 0; j < BlockCount; ++j)
                {
                    for (var k = 0; k < Depth; ++k)
                    {
                        _blocks[GetBlockIndex(i, j, k)] = -1;
                    }
                }
            }
        }

        public int GetBlockIndex(int x, int y, int z)
        {
            return x + (BlockCount * y) + (BlockCount * BlockCount * z);
        }

        public void SetBlockType(int x, int y, int z, short value)
        {
            var index = GetBlockIndex(x, y, z);

            if (_blocks[index] == value)
                return;

            _blocks[index] = value;
            if (BlocksUpdated != null)
                BlocksUpdated(this, x, y);
        }

        public short GetBlockType(int x, int y, int z)
        {
            var index = GetBlockIndex(x, y, z);
            return _blocks[index];
        }

        public short[] GetBlockData()
        {
            return _blocks;
        }

        // might need to be re-examined
        public void SetBlockData(short[] data)
        {
            for (var i = 0; i < BlockCount; ++i)
            {
                for (var j = 0; j < BlockCount; ++j)
                {
                    for (var k = 0; k < Depth; ++k)
                    {
                        var index = GetBlockIndex(i, j, k);
                        SetBlockType(i, j, k, data[index]);
                    }
                }
            }
        }
    }
}
