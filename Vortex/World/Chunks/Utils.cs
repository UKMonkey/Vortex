using System;
using SlimMath;
using Vortex.Interface;
using Vortex.Interface.World.Chunks;

namespace Vortex.World.Chunks
{
    static public class Utils
    {
        public static Vector3 GetChunkWorldVectorWithOffset(this IEngine engine, ChunkKey key, Vector3 offset = new Vector3())
        {
            return new Vector3(engine.ChunkWorldSize*key.X,
                              engine.ChunkWorldSize * key.Y, 0) + offset;
        }

        public static void GetChunkVectorFromWorldVector(this IEngine engine, Vector3 worldVector, out ChunkKey key, out Vector3 value) // keep as Vector3
        {
            key = engine.GetChunkKeyForPosition(worldVector);
            value = worldVector -
                    new Vector3(key.X * engine.ChunkWorldSize, key.Y * engine.ChunkWorldSize, 0);
        }

        public static ChunkKey GetChunkKeyForPosition(this IEngine engine, Vector3 point) // keep as Vector3
        {
            var x = (int)Math.Floor((point.X - engine.ChunkWorldSize) / (double)engine.ChunkWorldSize) + 1;
            var y = (int)Math.Floor((point.Y - engine.ChunkWorldSize) / (double)engine.ChunkWorldSize) + 1;
            return new ChunkKey(x, y);
        }

        public static Vector3 GetCentreOfChunk(this IEngine engine, ChunkKey key)
        {
            return engine.GetChunkWorldVectorWithOffset(key, new Vector3(engine.ChunkWorldSize / 2f, engine.ChunkWorldSize / 2f, 0));
        }
    }
}
