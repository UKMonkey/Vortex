using Vortex.Interface.Traits;
using SlimMath;

namespace Vortex.Interface.World.Blocks
{
    public class BlockProperty: Trait
    {
        public BlockProperty()
        {
        }

        public BlockProperty(Trait item) : base(item)
        {
        }

        public BlockProperty(short id, object data) : base(id, data)
        {
        }

        public BlockProperty(short id, byte[] data) : base(id, data)
        {
        }

        public BlockProperty(short id, byte data) : base(id, data)
        {
        }

        public BlockProperty(short id, bool data) : base(id, data)
        {
        }

        public BlockProperty(short id, int data) : base(id, data)
        {
        }

        public BlockProperty(short id, string data) : base(id, data)
        {
        }

        public BlockProperty(short id, short data) : base(id, data)
        {
        }

        public BlockProperty(short id, Vector3 data) : base(id, data)
        {
        }

        public BlockProperty(short id, float data) : base(id, data)
        {
        }

        public BlockProperty(short id, long data) : base(id, data)
        {
        }

        public BlockProperty(short id, double data) : base(id, data)
        {
        }


        public BlockProperty Clone()
        {
            var property = new BlockProperty
                {PropertyId = PropertyId, 
                 ByteArrayValue = ByteArrayValue};

            property.DataType = this.DataType;
            property.IsDirtyable = this.IsDirtyable;
            return property;
        }
    }
}
