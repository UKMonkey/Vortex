namespace Vortex.Interface.World.Blocks
{
    public static class BlockPropertyExtensions
    {
        /************************************************************************/
        /* Helpers                                                              */
        /************************************************************************/
        public static BlockProperty GetProperty(this BlockProperties props, BlockPropertyEnum prop)
        {
            return props.GetProperty((short)prop);
        }


        /************************************************************************/
        /* Block id                                                             */
        /************************************************************************/
        public static ushort GetBlockId(this BlockProperties props)
        {
            var trait = props.GetProperty(BlockPropertyEnum.BockId);
            return (ushort)(trait.ShortValue);
        }

        public static void SetBlockId(this BlockProperties props, ushort id)
        {
            var tmp = new BlockProperty((short)BlockPropertyEnum.BockId, id);
            props.SetProperty(tmp);
        }

        /************************************************************************/
        /* Material                                                             */
        /************************************************************************/
        public static void SetMaterial(this BlockProperties props, int material)
        {
            var tmp = new BlockProperty((short) BlockPropertyEnum.MaterialId, material);
            props.SetProperty(tmp);
        }
    }
}
