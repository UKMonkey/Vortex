using SlimMath;
using Vortex.Interface.Traits;

namespace Vortex.Interface.EntityBase.Properties
{
    public class EntityProperty : Trait
    {
        public EntityProperty()
        {
        }

        public EntityProperty(Trait item) : base(item)
        {
            
        }

        public EntityProperty(short id, object data) : base(id, data)
        {
        }

        public EntityProperty(short id, byte[] data) : base(id, data)
        {
        }

        public EntityProperty(short id, byte data) : base(id, data)
        {
        }

        public EntityProperty(short id, bool data) : base(id, data)
        {
        }

        public EntityProperty(short id, int data) : base(id, data)
        {
        }

        public EntityProperty(short id, string data) : base(id, data)
        {
        }

        public EntityProperty(short id, short data) : base(id, data)
        {
        }

        public EntityProperty(short id, Vector3 data) : base(id, data)
        {
        }

        public EntityProperty(short id, float data) : base(id, data)
        {
        }

        public EntityProperty(short id, long data) : base(id, data)
        {
        }

        public EntityProperty(short id, double data) : base(id, data)
        {
        }

        public EntityProperty(short id, Color4 data): base(id, data)
        {
        }

        public EntityProperty Clone()
        {
            var property = new EntityProperty
                {PropertyId = PropertyId, 
                 ByteArrayValue = ByteArrayValue};

            property.DataType = this.DataType;
            property.IsDirtyable = this.IsDirtyable;
            return property;
        }
    }
}
