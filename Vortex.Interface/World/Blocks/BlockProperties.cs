using Vortex.Interface.Traits;

namespace Vortex.Interface.World.Blocks
{
    public class BlockProperties: TraitCollection<BlockProperty>
    {
        private int _material;

        public BlockProperties()
        {
        }

        protected override void UpdateCachedProperties(Trait property)
        {
            switch (property.PropertyId)
            {
                case (short)BlockPropertyEnum.MaterialId:
                    _material = property.IntValue;
                    break;
            }
        }

        public int GetMaterial()
        {
            return _material;
        }
    }
}
