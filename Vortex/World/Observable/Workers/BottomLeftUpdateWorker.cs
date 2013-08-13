using SlimMath;
using Vortex.Interface.Debugging;
using Vortex.Interface.World.Chunks;

namespace Vortex.World.Observable.Workers
{
    public class BottomLeftUpdateWorker : IObservableAreaWorker
    {
        public TimingStats WorkOnArea(IObservableArea area)
        {
            var ret = new TimingStats("BottomLeftUpdateWorker");

            ret.StartingTask("Work");
            var chunksObserved = area.ChunksObservedBuffer;
            var bottomLeft = new Vector2(chunksObserved[0][0].X * Chunk.ChunkWorldSize,
                         chunksObserved[0][0].Y * Chunk.ChunkWorldSize);

            area.BottomLeftBuffer = bottomLeft;
            ret.CompletedTask("Work");

            return ret;
        }
    }
}
