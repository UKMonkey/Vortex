using System.Collections.Generic;

namespace Beer.World
{
    public class Chunk
    {
        public const int Size = 16;
        public readonly ChunkKey Key;
        public List<List<Tile>> Tiles { get; private set; }
        // Static lights
        public List<ILight> Lights { get; private set; }

        public Chunk(ChunkKey key, List<List<Tile>> tiles, List<ILight> lights)
        {
            Key = key;
            Tiles = tiles;
            Lights = lights;
        }
    }
}
