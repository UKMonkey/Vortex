using System.Collections.Generic;
using Beer.World.Chunks;
using Beer.World.Interfaces.Chunks;

namespace Beer.World.Providers
{
    class RandomProvider : IChunkLoader
    {
        public ChunkCallback OnChunkLoad { get; set; }
        public ChunkCallback OnChunksGenerated { get; set; }
        public ChunkKeyCallback OnChunksUnavailable { get; set; }

        private int _count;

        private readonly List<IChunkLoader> _loaders; 

        public RandomProvider(List<IChunkLoader> loaders)
        {
            _loaders = loaders;
            foreach(var item in _loaders)
            {
                item.OnChunkLoad += ChunksLoaded;
                item.OnChunksGenerated += ChunksGenerated;
                item.OnChunksUnavailable += ChunksUnavailable;
            }
        }

        public void LoadChunks(List<ChunkKey> chunkKeys)
        {
            var index = (_count++)%_loaders.Count;
            _loaders[index].LoadChunks(chunkKeys);
        }

        private void ChunksLoaded(List<Chunk> chunks)
        {
            if (OnChunkLoad != null)
                OnChunkLoad(chunks);
        }

        private void ChunksGenerated(List<Chunk> chunks)
        {
            if (OnChunksGenerated != null)
                OnChunksGenerated(chunks);
        }

        private void ChunksUnavailable(List<ChunkKey> chunks)
        {
            if (OnChunksUnavailable != null)
                OnChunksUnavailable(chunks);
        }
    }
}
