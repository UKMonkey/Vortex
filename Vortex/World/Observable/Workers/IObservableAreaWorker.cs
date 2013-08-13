using Vortex.Interface.Debugging;

namespace Vortex.World.Observable.Workers
{
    public interface IObservableAreaWorker
    {
        TimingStats WorkOnArea(IObservableArea area);
    }
}
