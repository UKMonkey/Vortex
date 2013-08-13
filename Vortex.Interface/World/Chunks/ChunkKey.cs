using Psy.Core;
using SlimMath;
using Vector3 = SlimMath.Vector3;

namespace Vortex.Interface.World.Chunks
{
    public struct ChunkKey
    {
        public readonly int X;
        public readonly int Y;

        public ChunkKey(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Rectangle GetWorldArea(IEngine engine)
        {
            var bottomLeft = engine.ChunkVectorToWorldVector(this, new Vector3(0, 0, 0));

            return new Rectangle(new Vector2(bottomLeft.X, bottomLeft.Y + Chunk.ChunkWorldSize),
                                 new Vector2(bottomLeft.X + Chunk.ChunkWorldSize, bottomLeft.Y));
        }

        public override int GetHashCode()
        {
            // just make it so that X,Y isn't the same as Y,X else it'll look crap in the middle
            return (X << 5)*Y;
        }

        public static bool operator == (ChunkKey a, ChunkKey b)
        {
            return (a.X == b.X && a.Y == b.Y);
        }

        public static bool operator !=(ChunkKey a, ChunkKey b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public ChunkKey Left()
        {
            return new ChunkKey(X-1, Y);
        }

        public ChunkKey Right()
        {
            return new ChunkKey(X+1, Y);
        }

        public ChunkKey Top()
        {
            return new ChunkKey(X, Y+1);
        }

        public ChunkKey Bottom()
        {
            return new ChunkKey(X, Y-1);
        }

        public override string ToString()
        {
            return string.Format("ChunkKey[{0}, {1}]", X, Y);
        }
    }
}
