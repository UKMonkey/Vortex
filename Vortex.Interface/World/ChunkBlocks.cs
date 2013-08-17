namespace Vortex.Interface.World
{
    public delegate void BlockCallback(ChunkBlocks blocks, short x, short y, short z);

    public class ChunkBlocks
    {
        public event BlockCallback BlocksUpdated;

        private const int BlockCount = 16;
        private const int Depth = 1024;
        private readonly short [] _blocks;

        public ChunkBlocks()
        {
            _blocks = new short[BlockCount * BlockCount * Depth];
            for (short i = 0; i < BlockCount; ++i)
            {
                for (short j = 0; j < BlockCount; ++j)
                {
                    for (short k = 0; k < Depth; ++k)
                    {
                        _blocks[GetBlockIndex(i, j, k)] = -1;
                    }
                }
            }
        }

        public int GetBlockIndex(short x, short y, short z)
        {
            return x + (BlockCount * y) + (BlockCount * BlockCount * z);
        }

        public void SetBlockType(short x, short y, short z, short value)
        {
            var index = GetBlockIndex(x, y, z);

            if (_blocks[index] == value)
                return;

            _blocks[index] = value;
            if (BlocksUpdated != null)
                BlocksUpdated(this, x, y, z);
        }

        public short GetBlockType(short x, short y, short z)
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
            for (short i = 0; i < BlockCount; ++i)
            {
                for (short j = 0; j < BlockCount; ++j)
                {
                    for (short k = 0; k < Depth; ++k)
                    {
                        var index = GetBlockIndex(i, j, k);
                        SetBlockType(i, j, k, data[index]);
                    }
                }
            }
        }
    }
}
