namespace Vortex.Interface.World.Blocks
{
    public interface IBlockTypeCache
    {
        /// <summary>
        /// for a given type of block, these are its properties
        /// they should never be changed while the game is running
        /// </summary>
        /// <param name="id"></param>
        /// <param name="props"></param>
        void RegisterProperties(BlockProperties props);

        /// <summary>
        /// get the properties for a given type of block
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        BlockProperties GetBlockProperties(ushort id);
    }
}
