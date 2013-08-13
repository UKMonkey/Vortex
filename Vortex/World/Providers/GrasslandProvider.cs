using System.Collections.Generic;
using Beer.World.Chunks;
using Beer.World.Interfaces.Chunks;
using Psy.Core;

namespace Beer.World.Providers
{
    class GrasslandProvider : IChunkLoader
    {
        public ChunkCallback OnChunkLoad { get; set; }
        public ChunkCallback OnChunksGenerated { get; set; }
        public ChunkKeyCallback OnChunksUnavailable { get; set; }

        public void LoadChunks(List<ChunkKey> keys)
        {
            var generated = new List<Chunk>();

            foreach (var key in keys)
            {
                var tiles = new List<List<Tile>>();
                var lights = new List<ILight>
                                 {
                                     new Light(GetBottomLeft(key, Chunk.TilesPerChunk, NewMap.TileSize), 2,
                                               Colours.RandomSolid()),
                                     new Light(GetBottomLeft(key, Chunk.TilesPerChunk, NewMap.TileSize) + new Vector(1,1), 2,
                                               Colours.RandomSolid()),
                                     new Light(GetBottomLeft(key, Chunk.TilesPerChunk, NewMap.TileSize) + new Vector(1,4), 2,
                                               Colours.RandomSolid()),
                                     new Light(GetBottomLeft(key, Chunk.TilesPerChunk, NewMap.TileSize) + new Vector(4,1), 2,
                                               Colours.RandomSolid()),
                                     new Light(GetBottomLeft(key, Chunk.TilesPerChunk, NewMap.TileSize) + new Vector(2,2), 2,
                                               Colours.RandomSolid())
                                 };
                for (var i = 0; i < Chunk.TilesPerChunk; ++i)
                {
                    tiles.Add(new List<Tile>());
                    for (var j = 0; j < Chunk.TilesPerChunk; ++j)
                    {
                        var tile = new Tile {Height = 0, IsOutside = true};
                        foreach (var face in tile.Faces)
                        {
                            face.TilesetReference = 0;
                        }
                        //if (i == 0 || j == 0)
                        //{
                        //    tile.IsOutside = false;
                        //    tile.Height = 100;
                        //}
                        tiles[i].Add(tile);
                    }
                }

                tiles[0][0].Faces[(int) TileFaceEnum.Top].TilesetReference = 2;
                tiles[1][0].Faces[(int)TileFaceEnum.Top].TilesetReference = 2;
                tiles[0][1].Faces[(int)TileFaceEnum.Top].TilesetReference = 2;

                tiles[1][1].Height = 128;

 
                var toAdd = new Chunk(key, tiles, lights);

                generated.Add(toAdd);
            }
            OnChunksGenerated(generated);
        }


        private static Vector GetBottomLeft(ChunkKey key, int chunkSize, float tileSize)
        {
            return new Vector(0, 0);
            //return new Vector(key.X * chunkSize * tileSize,
            //                  key.Y * chunkSize * tileSize);
        }
    }
}
