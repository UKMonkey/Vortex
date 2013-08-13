using System.Linq;
using SlimMath;
using Vortex.Interface.Debugging;
using Vortex.Interface.World;
using Vortex.Interface.World.Chunks;

namespace Vortex.World.Observable.Workers
{
    class LightsUpdateWorker : IObservableAreaWorker
    {
        private readonly IChunkCache _chunkCache;


        public LightsUpdateWorker(IChunkCache cache)
        {
            _chunkCache = cache;
        }

        public TimingStats WorkOnArea(IObservableArea area)
        {
            var ret = new TimingStats("Update lights");

            area.LightsBuffer.Clear();
            var chunkKeys = area.ChunksObservedBuffer;
            var chunks = _chunkCache.GetChunks(chunkKeys.SelectMany(item => item).ToList());

            ret.StartingTask("Work");
            foreach (var chunk in chunks)
            {
                foreach (var light in chunk.Lights)
                {
                    var toAdd = new Light(light.Position, light.Brightness, light.Colour);
                    toAdd.Position += new Vector3(chunk.Key.X, chunk.Key.Y, 0) * Chunk.ChunkWorldSize;

                    area.LightsBuffer.Add(toAdd);
                }
            }
            ret.CompletedTask("Work");

            return ret;
        }
    }
}
