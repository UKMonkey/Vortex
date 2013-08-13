using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Beer.World.Interfaces;

namespace Beer.Client.World
{
    public class ChunkLoader : IChunkLoader
    {
        public ChunkLoader()
        {
        }

        // send a message to get an update ... wait for the reply
        // return the chunk
        //
        // NOTE: THIS IS SLOW  -  call on a seperate thread, it's slow!
        public Chunk LoadChunk(int x, int y)
        {
            
        }

        // do nothing
        public void SaveChunk(Chunk toSave)
        {
        }
    }
}
