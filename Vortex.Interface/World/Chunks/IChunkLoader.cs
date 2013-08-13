using System;
using System.Collections.Generic;

namespace Vortex.Interface.World.Chunks
{
    public delegate void ChunkCallback(List<Chunk> items);
    public delegate void ChunkKeyCallback(List<ChunkKey> items);

    public interface IChunkLoader : IDisposable
    {
        /// <summary>
        /// Called on (probably) another thread - anything inside this must be thread safe
        /// Called when a chunk has been loaded and is available 
        /// </summary>
        event ChunkCallback OnChunkLoad;

        /// <summary>
        /// Called on (probably) another thread - anything inside this must be thread safe
        /// Called when a chunk has been created from scratch
        /// </summary>
        event ChunkCallback OnChunksGenerated;

        /// <summary>
        /// Called on (probably) another thread - anything inside this must be thread safe
        /// Called when a chunk is definately not available and must be generated
        /// </summary>
        event ChunkKeyCallback OnChunksUnavailable;

        /// <summary>
        /// Request a load of chunks to be loaded
        /// </summary>
        /// <param name="chunkKeys">keys to load</param>
        void LoadChunks(List<ChunkKey> chunkKeys);
    }
}
