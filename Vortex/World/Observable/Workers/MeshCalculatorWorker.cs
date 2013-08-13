using Vortex.Interface.Debugging;

namespace Vortex.World.Observable.Workers
{
    public class MeshCalculatorWorker : IObservableAreaWorker
    {
        public const float TileHeightMultiplier = 128.0f;

        public TimingStats WorkOnArea(IObservableArea area)
        {
            return new TimingStats("Unused");
        }
    }
}
