using System;
using System.Collections.Generic;

namespace Vortex.Interface.World.Chunks
{
    public interface IChunkSaver : IDisposable
    {
        /// <summary>
        /// Request a load of chunks to be saved
        /// </summary>
        /// <param name="chunksToSave">chunks to save somewhere</param>
        void SaveChunks(List<Chunk> chunksToSave);
    }
}
