namespace Vortex.Interface.World
{
    public delegate void BlockCallback(ChunkBlocks blocks, int x, int y);

    public class ChunkBlocks
    {
        public event BlockCallback BlocksUpdated;

        private const int BlockCount = 16;
        private readonly short [,] _blocks;

        public ChunkBlocks()
        {
            _blocks = new short[BlockCount, BlockCount];
            for (var i = 0; i < BlockCount; ++i)
            {
                for (var j = 0; j < BlockCount; ++j)
                {
                    _blocks[i, j] = 0;
                }
            }
        }

        public void SetBlockType(int x, int y, short value)
        {
            if (_blocks[x, y] == value)
                return;

            _blocks[x, y] = value;
            if (BlocksUpdated != null)
                BlocksUpdated(this, x, y);
        }

        public short GetBlockType(int x, int y)
        {
            return _blocks[x, y];
        }
    }
}
