using System.Collections.Generic;
using Vortex.Interface.World.Chunks;

namespace Vortex.World.Chunks
{
    abstract class ChunkProvider : IChunkLoader
    {
        private readonly List<IChunkLoader> _loaders;
        public event ChunkCallback OnChunkLoad;
        public event ChunkKeyCallback OnChunksUnavailable;
        public event ChunkCallback OnChunksGenerated;

        private readonly Dictionary<ChunkKey, int> _keyToLastLoader;

        public abstract void Dispose();

        protected ChunkProvider(List<IChunkLoader> loaders)
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

        // Load the next key available ....
        public void LoadChunks(List<ChunkKey> keys)
        {
            foreach (var key in keys)
            {
                _keyToLastLoader.Add(key, 0);
            }

            _loaders[0].LoadChunks(keys);
        }
        
        private void ChunksLoaded(List<Chunk> chunks)
        {
            foreach (var chunk in chunks)
            {
                _keyToLastLoader.Remove(chunk.Key);
            }

            if (OnChunkLoad != null)
                OnChunkLoad(chunks);
        }

        private void ChunksGenerated(List<Chunk> chunks)
        {
            foreach (var chunk in chunks)
            {
                _keyToLastLoader.Remove(chunk.Key);
            }

            if (OnChunksGenerated != null)
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
                var toUse = _keyToLastLoader[key] + 1;
                if (toUse == _loaders.Count)
                {
                    unavailable.Add(key);
                }
                else
                {
                    if (!toReload.ContainsKey(toUse))
                    {
                        toReload.Add(toUse, new List<ChunkKey>());
                    }
                    toReload[toUse].Add(key);
                }
            }

            if (unavailable.Count > 0 && OnChunksUnavailable != null)
            {
                OnChunksUnavailable(unavailable);
            }

            foreach (var item in toReload)
            {
                _loaders[item.Key].LoadChunks(item.Value);
            }
        }
    }
}
