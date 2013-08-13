using System.Collections.Generic;
using Beer.World.Interfaces;

namespace Beer.World
{
    class ChunkProvider : IChunkLoader
    {
        private readonly List<IChunkLoader> _loaders;
        public ChunkCallback OnChunkLoad { get; set; }
        public ChunkKeyCallback OnChunksUnavailable { get; set; }
        public ChunkCallback OnChunksGenerated { get; set; }

        private readonly Dictionary<ChunkKey, int> _keyToLastLoader; 

        public ChunkProvider(List<IChunkLoader> loaders)
        {
            _loaders = loaders;
            _keyToLastLoader = new Dictionary<ChunkKey, int>();
            foreach(var loader in loaders)
            {
                loader.OnChunksGenerated += ChunksGenerated;
                loader.OnChunkLoad += ChunksLoaded;
                loader.OnChunksUnavailable += ChunksUnavailable;
            }
        }

        public void Dispose()
        {
            foreach (var chunkLoader in _loaders)
            {
                chunkLoader.Dispose();
            }
        }

        // Load the next key available ....
        public void LoadChunks(List<ChunkKey> chunkKeys)
        {
            foreach (ChunkKey key in chunkKeys)
                _keyToLastLoader.Add(key, 0);

            _loaders[0].LoadChunks(chunkKeys);
        }
        
        private void ChunksLoaded(List<Chunk> chunks)
        {
            foreach (var chunk in chunks)
                _keyToLastLoader.Remove(chunk.Key);
            
            OnChunkLoad(chunks);
        }

        private void ChunksGenerated(List<Chunk> chunks)
        {
            foreach (var chunk in chunks)
                _keyToLastLoader.Remove(chunk.Key);

            OnChunksGenerated(chunks);
        }

        private void ChunksUnavailable(List<ChunkKey> keys)
        {
            // the chunks aren't really unavailable - just unavailable for the last loader we tried (maybe)
            // we'll try all the loaders before calling 
            // OnChunksUnavailable(keys);
            var toReload = new Dictionary<int, List<ChunkKey>>();
            var unavailable = new List<ChunkKey>();

            foreach (var key in keys)
            {
                int toUse = _keyToLastLoader[key] + 1;
                if (toUse == _loaders.Count)
                {
                    unavailable.Add(key);
                }
                else
                {
                    if (!toReload.ContainsKey(toUse))
                        toReload.Add(toUse, new List<ChunkKey>());
                    toReload[toUse].Add(key);
                }
            }

            if (unavailable.Count > 0)
                OnChunksUnavailable(unavailable);
            foreach (KeyValuePair<int, List<ChunkKey>> item in toReload)
                _loaders[item.Key].LoadChunks(item.Value);
        }
    }
}
