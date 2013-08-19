using SlimMath;
using Vortex.Interface;
using Vortex.Interface.Debugging;

namespace Vortex.World.Observable.Workers
{
    public class BottomLeftUpdateWorker : IObservableAreaWorker
    {
        private IEngine Engine { get; set; }

        public BottomLeftUpdateWorker(IEngine engine)
        {
            Engine = engine;
        }

        public TimingStats WorkOnArea(IObservableArea area)
        {
            var ret = new TimingStats("BottomLeftUpdateWorker");

            ret.StartingTask("Work");
            var chunksObserved = area.ChunksObservedBuffer;
            var bottomLeft = new Vector2(chunksObserved[0][0].X * Engine.ChunkWorldSize,
                         chunksObserved[0][0].Y * Engine.ChunkWorldSize);

            area.BottomLeftBuffer = bottomLeft;
            ret.CompletedTask("Work");

            return ret;
        }
    }
}
