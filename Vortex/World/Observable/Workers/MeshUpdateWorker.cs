using System.Linq;
﻿using System.Collections.Generic;
using Vortex.Interface;
using Vortex.Interface.Debugging;
using Vortex.Interface.World;
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
                var meshes = chunk.GetChunkMeshes();
                foreach (var meshPair in meshes)
                {
                    meshPair.Value.WorldVector = _engine.GetChunkWorldVectorWithOffset(chunk.Key);
                    meshBuffer.Add(meshPair.Value);
                }
            }
            ret.CompletedTask("Adding data");

            return ret;
        }
    }
}
