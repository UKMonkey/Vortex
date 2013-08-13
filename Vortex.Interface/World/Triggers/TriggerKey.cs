using Vortex.Interface.World.Chunks;

namespace Vortex.Interface.World.Triggers
{
    public class TriggerKey
    {
        public ChunkKey ChunkLocation { get; private set; }
        public short Id { get; private set; }

        public TriggerKey(ChunkKey key, short id)
        {
            ChunkLocation = key;
            Id = id;
        }

        public static bool operator == (TriggerKey a, TriggerKey b)
        {
            var aIsNull = System.Object.ReferenceEquals(a, null);
            var bIsNull = System.Object.ReferenceEquals(b, null);

            if (aIsNull || bIsNull)
            {
                return aIsNull && bIsNull;
            }

            return (a.Id == b.Id) && (a.ChunkLocation == b.ChunkLocation);
        }

        public static bool operator !=(TriggerKey a, TriggerKey b)
        {
            return !(a == b);
        }


        /** Unused functions to shut up the compiler
         */
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(TriggerKey)) return false;
            return Equals((TriggerKey)obj);
        }

        private bool Equals(TriggerKey other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.ChunkLocation.Equals(ChunkLocation) && other.Id == Id;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (ChunkLocation.GetHashCode()*397) ^ Id.GetHashCode();
            }
        }
    }
}
