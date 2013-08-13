using System.Collections.Generic;
using Beer.World.Interfaces;

namespace Beer.World
{
    public class ChunkCache: IChunkCache
    {
        public ChunkCallback OnChunksLoaded { get; set; }
        public ChunkCallback OnChunksUpdated { get; set; }

        private readonly IChunkSaver _chunkSaver;
        private readonly IChunkLoader _chunkLoader;
        private readonly Dictionary<ChunkKey, Chunk> _cache;
        
        private readonly List<Chunk> _updatedChunks;
        private readonly List<Chunk> _loadedChunks;

        protected ChunkCache(IChunkLoader loader, IChunkSaver saver)
        {
            _cache = new Dictionary<ChunkKey, Chunk>();

            _updatedChunks = new List<Chunk>();
            _loadedChunks = new List<Chunk>();

            _chunkSaver = saver;
            _chunkLoader = loader;
            _chunkLoader.OnChunkLoad += ChunksLoaded;
            _chunkLoader.OnChunksGenerated += ChunksGenerated;
        }

        /** Get any chunks in memory
         *  if it's not available then request it from the loader
         */
        public List<Chunk> GetChunks(List<ChunkKey> keys)
        {
            var toLoad = new List<ChunkKey>();
            var available = new List<Chunk>();

            foreach (var key in keys)
            {
                if (_cache.ContainsKey(key))
                {
                    available.Add(_cache[key]);
                }
                else
                {
                    toLoad.Add(key);
                }
            }

            _chunkLoader.LoadChunks(toLoad);

            return available;
        }

        /** Notify the chunk loader to save chunks
         *  Register everything changed and we'll deal with it later
         */
        public void UpdateChunks(List<Chunk> changedChunks)
        {
            if (changedChunks.Count == 0)
            {
                return;
            }

            lock (this)
            {
                _updatedChunks.InsertRange(_updatedChunks.Count, changedChunks);
            }
            _chunkSaver.SaveChunks(changedChunks);
        }

        private void ChunksGenerated(List<Chunk> chunks)
        {
            lock (this)
            {
                _chunkSaver.SaveChunks(chunks);
            }
            ChunksLoaded(chunks);
        }

        /** Called by the loader when chunks have been loaded
         *  Do nothing - deal with it later
         */
        private void ChunksLoaded(List<Chunk> chunks)
        {
            lock (this)
            {
                _updatedChunks.InsertRange(_loadedChunks.Count, chunks);
            }
        }

        /** Deal with anything that's been loaded or updated
         */
        public void ProcessLoadedChunks()
        {
            ChunksUpdated(_updatedChunks, false);
            ChunksUpdated(_loadedChunks, true);
        }


        /******************************************/

        /** sends notifications to 'OnChunksLoaded' or 'OnChunksUpdated' for anything changed since last call.
         */
        private void ChunksUpdated(List<Chunk> changedChunks, bool isNew)
        {
            List<Chunk> copy;
            lock (this)
            {
                copy = new List<Chunk>(changedChunks);

                foreach (Chunk chunk in changedChunks)
                {
                    _cache[chunk.Key] = chunk;
                }

                changedChunks.Clear();
            }

            if (isNew)
            {
                if (OnChunksLoaded != null)
                    OnChunksLoaded(copy);
            }
            else
            {
                if (OnChunksUpdated != null)
                    OnChunksUpdated(copy);
            }
        }
    }
}
