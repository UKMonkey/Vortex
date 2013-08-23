using System.Collections.Generic;

namespace Vortex.Interface.World.Chunks
{
    public delegate void SingleChunkCallback(IChunk chunk);

    public interface IChunk
    {
        event SingleChunkCallback ChunkChanged;

        /// <summary>
        /// Get/Set the key for this chunk
        /// </summary>
        ChunkKey Key { get; set; }

        /// <summary>
        /// lights in the chunk
        /// </summary>
        List<ILight> Lights { get; }

        /// <summary>
        /// Returns the full details of what this chunk contains (ignoring lights)
        /// </summary>
        /// <returns></returns>
        byte[] GetFullData();

        /// <summary>
        /// Used to apply data returned by GetFullData
        /// </summary>
        /// <param name="data"></param>
        void ApplyFullData(byte[] data);

        /// <summary>
        /// Gets the smallest amount of data that when applied ontop of the last update
        /// will get to this item.  This may be the full data, or a tiny amount.
        /// </summary>
        /// <returns></returns>
        byte[] GetDirtyData();

        /// <summary>
        /// Used to apply dirty data.
        /// </summary>
        /// <param name="data"></param>
        void ApplyDirtyData(byte[] data);

        /// <summary>
        /// get the chunk to re-establish the mesh.
        /// the chunk MUST NOT recalculate the mesh if it's changed
        /// if it's changed, it should use the event to notify the rest of the engine
        /// and then the engine will decide if the mesh should be re-calculated or not
        /// (If the mesh has already been updated, and that was the change then it's assumed
        /// this function will do nothing)
        /// </summary>
        /// <param name="engine"></param>
        void RecalculateMesh(IEngine engine);

        /// <summary>
        /// If the chunk wishes to be displayed differently depending on the 'level of interest'
        /// then it can choose how to do so here.
        /// </summary>
        ChunkMesh GetChunkMesh(int levelOfInterest);

        /// <summary>
        /// pulls out all of the meshes with their level of interest
        /// </summary>
        /// <returns></returns>
        IEnumerable<KeyValuePair<int, ChunkMesh>> GetChunkMeshes();
    }
}
