using Vortex.Interface.Traits;

namespace Vortex.Interface.World.Blocks
{
    public class BlockProperties: TraitCollection<BlockProperty>
    {
        public BlockProperties()
        {
            // TODO - establish what we want cached & provide custom functions to return that data directly
            // ie anything that'll be used in the render loop!
        }

        protected override void UpdateCachedProperties(Trait property)
        {}
    }
}
