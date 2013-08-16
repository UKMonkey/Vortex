using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        public static short GetBlockId(this BlockProperties props)
        {
            var trait = props.GetProperty(BlockPropertyEnum.BockId);
            return trait.ShortValue;
        }

        public static void SetBlockId(this BlockProperties props, short id)
        {
            var tmp = new BlockProperty((short)BlockPropertyEnum.BockId, id);
            props.SetProperty(tmp);
        }
    }
}
