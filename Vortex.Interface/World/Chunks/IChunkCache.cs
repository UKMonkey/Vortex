using System.Collections.Generic;

namespace Vortex.Interface.World.Chunks
{
    public interface IChunkCache
    {
        /// <summary>
        /// when chunks already possibly loaded are changed, this function is called to notify anything interested.
        /// </summary>
        event ChunkCallback OnChunksUpdated;

        /// <summary>
        /// when Chunks are loaded by the loader for the first time, this function is called.
        /// </summary>
        event ChunkCallback OnChunksLoaded;

        /// <summary>
        /// Return any chunks in the cache that we have.  Request the loader to get any others.
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        List<Chunk> GetChunks(IEnumerable<ChunkKey> keys);

        /// <summary>
        /// Notify the cache that chunks that have already been generated have changed.
        /// </summary>
        /// <param name="changedChunk"></param>
        void UpdateChunks(List<Chunk> changedChunk);

        /// <summary>
        /// Once complete, any stored chunks waiting to be pushed to "OnChunksUpdated" or "OnChunksLoaded" will be guarenteed to have been done.
        /// </summary>
        void ProcessLoadedData();
    }
}
