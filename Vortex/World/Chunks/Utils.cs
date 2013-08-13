using System;
using SlimMath;
using Vortex.Interface.World.Chunks;

namespace Vortex.World.Chunks
{
    static public class Utils
    {
        public static Vector3 GetChunkWorldVectorWithOffset(ChunkKey key, Vector3 offset = new Vector3())
        {
            return new Vector3(Chunk.ChunkWorldSize*key.X,
                              Chunk.ChunkWorldSize*key.Y, 0) + offset;
        }

        public static void GetChunkVectorFromWorldVector(Vector3 worldVector, out ChunkKey key, out Vector3 value) // keep as Vector3
        {
            key = GetChunkKeyForPosition(worldVector);
            value = worldVector -
                    new Vector3(key.X * Chunk.ChunkWorldSize, key.Y * Chunk.ChunkWorldSize, 0);
        }

        public static ChunkKey GetChunkKeyForPosition(Vector3 point) // keep as Vector3
        {
            var x = (int) Math.Floor((point.X - Chunk.ChunkWorldSize)/(double) Chunk.ChunkWorldSize) + 1;
            var y = (int) Math.Floor((point.Y - Chunk.ChunkWorldSize)/(double) Chunk.ChunkWorldSize) + 1;
            return new ChunkKey(x, y);
        }
    }
}
