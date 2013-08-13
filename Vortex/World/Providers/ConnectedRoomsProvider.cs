using System.Collections.Generic;
using Beer.World.Chunks;
using Beer.World.Interfaces.Chunks;
using Psy.Core;

namespace Beer.World.Providers
{
    public class ConnectedRoomsProvider : IChunkLoader
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
                                     new Light(new Vector(2.0f, 2.0f), 5,
                                               Colours.RandomSolid())
                                 };
                for (var i = 0; i < Chunk.TilesPerChunk; ++i)
                {
                    tiles.Add(new List<Tile>());
                    for (var j = 0; j < Chunk.TilesPerChunk; ++j)
                    {
                        var tile = new Tile { Height = 0, IsOutside = false };
                        foreach (var face in tile.Faces)
                        {
                            face.TilesetReference = 0;
                        }

                        tiles[i].Add(tile);
                    }
                }

                MakeWall(tiles[0][0]);
                MakeWall(tiles[0][1]);
                MakeWall(tiles[0][2]);
                MakeWall(tiles[0][3]);
                MakeWall(tiles[0][4]);
                MakeWall(tiles[0][5]);

                MakeWall(tiles[0][10]);
                MakeWall(tiles[0][11]);
                MakeWall(tiles[0][12]);
                MakeWall(tiles[0][13]);
                MakeWall(tiles[0][14]);
                MakeWall(tiles[0][15]);

                MakeWall(tiles[1][0]);
                MakeWall(tiles[2][0]);
                MakeWall(tiles[3][0]);
                MakeWall(tiles[4][0]);
                MakeWall(tiles[5][0]);

                MakeWall(tiles[10][0]);
                MakeWall(tiles[11][0]);
                MakeWall(tiles[12][0]);
                MakeWall(tiles[13][0]);
                MakeWall(tiles[14][0]);
                MakeWall(tiles[15][0]);

                

                MakeWall(tiles[3][6]);
                MakeWall(tiles[3][7]);
                MakeWall(tiles[3][8]);
                MakeWall(tiles[3][9]);


                MakeWall(tiles[11][6]);
                MakeWall(tiles[11][7]);
                MakeWall(tiles[11][8]);
                MakeWall(tiles[11][9]);




                var toAdd = new Chunk(key, tiles, lights);

                generated.Add(toAdd);
            }
            OnChunksGenerated(generated);
        }

        private static void MakeWall(Tile tile)
        {
            tile.Faces[(int)TileFaceEnum.Top].TilesetReference = 4;
            tile.Faces[(int)TileFaceEnum.Front].TilesetReference = 4;
            tile.Faces[(int)TileFaceEnum.Back].TilesetReference = 4;
            tile.Faces[(int)TileFaceEnum.Left].TilesetReference = 4;
            tile.Faces[(int)TileFaceEnum.Right].TilesetReference = 4;
            tile.Height = 128;
        }
    }
}