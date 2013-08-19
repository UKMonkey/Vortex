﻿using System.Linq;
using Vortex.Interface;
using Vortex.Interface.Debugging;
using Vortex.Interface.World.Chunks;
using Vortex.World.Chunks;

namespace Vortex.World.Observable.Workers
{
    /**
     * Responsible for updating the secondary buffer tiles
     */
    class MeshUpdateWorker : IObservableAreaWorker
    {
        private readonly IChunkCache _chunkCache;
        private readonly IEngine _engine;

        public MeshUpdateWorker(IChunkCache cache, IEngine engine)
        {
            _chunkCache = cache;
            _engine = engine;
        }

        public TimingStats WorkOnArea(IObservableArea area)
        {
            var ret = new TimingStats("MeshUpdating");

            ret.StartingTask("Collecting data");
            var meshBuffer = area.ChunkMeshesBuffer;
            var chunksObservedBuffer = area.ChunksObservedBuffer;
            var listOfChunks = chunksObservedBuffer.SelectMany(chunk => chunk);

            var chunks = _chunkCache.GetChunks(listOfChunks);
            ret.CompletedTask("Collecting data");

            ret.StartingTask("Adding data");
            meshBuffer.Clear();
            foreach (var chunk in chunks)
            {
                chunk.ChunkMesh.WorldVector = _engine.GetChunkWorldVectorWithOffset(chunk.Key);
                meshBuffer.Add(chunk.ChunkMesh);
            }
            ret.CompletedTask("Adding data");

            return ret;
        }
    }
}
