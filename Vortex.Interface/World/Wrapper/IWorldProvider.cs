using Vortex.Interface.World.Chunks;
using Vortex.Interface.World.Entities;
using Vortex.Interface.World.Triggers;

namespace Vortex.Interface.World.Wrapper
{
    public interface IWorldProvider : IChunkLoader, IEntityLoader, ITriggerLoader
    {
    }
}
