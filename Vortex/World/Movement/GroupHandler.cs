using System.Collections.Generic;
using Vortex.Interface.EntityBase;

namespace Vortex.World.Movement
{
    public class GroupHandler : IMovementHandler
    {
        private readonly List<IMovementHandler> _handlers;

        public World World
        {
            set 
            {
                foreach (var item in _handlers)
                    item.World = value;
            }
        }


        public GroupHandler(IEnumerable<IMovementHandler> handlers)
        {
            _handlers = new List<IMovementHandler>(handlers);
        }

        public bool IsEntityMoving(Entity item)
        {
            foreach (var handler in _handlers)
                if (handler.IsEntityMoving(item))
                    return true;
            return false;
        }

        public bool HandleEntityMovement(Entity item)
        {
            var moved = false;

            foreach (var handler in _handlers)
            {
                if (handler.IsEntityMoving(item))
                    moved |= handler.HandleEntityMovement(item);
            }

            return moved;
        }
    }
}
