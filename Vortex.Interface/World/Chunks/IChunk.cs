using System.Collections.Generic;

namespace Vortex.Interface.World.Chunks
{
    public delegate void SingleChunkCallback(IChunk chunk);

    public interface IChunk
    {
        event SingleChunkCallback ChunkChanged;

        /// <summary>
        /// Returns the type of chunk this is.  The type is decided by the factory and is 
        /// used to ensure that the client creates the right class of chunk to handle the 
        /// dirty data messages
        /// </summary>
        short Type { get; }

        /// <summary>
        /// Get/Set the key for this chunk
        /// </summary>
        ChunkKey Key { get; set; }

        /// <summary>
        /// lights in the chunk
        /// </summary>
        List<ILight> Lights { get; }

        /// <summary>
        /// How this chunk should be drawn for the given level of interest
        /// </summary>
        ChunkMesh ChunkMesh { get; }

        /// <summary>
        /// In the case where there are multiple levels, this
        /// sets which we're interested in.  The Chunk may decide
        /// to provide a mesh that covers MORE than just this level, but that's
        /// for the chunk to decide how it wants to be drawn
        /// </summary>
        short LevelOfInterest { set; }

        /// <summary>
        /// Returns the full details of what this chunk contains (ignoring lights)
        /// </summary>
        /// <returns></returns>
        byte[] GetFullData();

        /// <summary>
        /// Gets the smallest amount of data that when applied ontop of the last update
        /// will get to this item.  This may be the full data, or a tiny amount.
        /// </summary>
        /// <returns></returns>
        byte[] GetDirtyData();

        /// <summary>
        /// Used to apply either/both the full and dirty data.
        /// </summary>
        /// <param name="data"></param>
        void ApplyData(byte[] data);
    }
}
