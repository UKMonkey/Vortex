using System.Collections.Generic;
using System.Linq;
using SlimMath;
using Vortex.Interface.EntityBase;
using Vortex.Interface.World.Chunks;

namespace Vortex.World.Quad
{
    public class LockedQuadTree : IQuadTree
    {
        private readonly IQuadTree _tree;

        public LockedQuadTree(IQuadTree tree)
        {
            _tree = tree;
        }

        public void UpdateItems(IEnumerable<Entity> items)
        {
            lock (_tree)
            {
                _tree.UpdateItems(items);
            }
        }

        public void UpdateItem(Entity changed)
        {
            lock (_tree)
            {
                _tree.UpdateItem(changed);
            }
        }

        public void InsertItem(ChunkKey area, Entity item)
        {
            lock (_tree)
            {
                _tree.InsertItem(area, item);
            }
        }

        public void InsertItem(Entity item)
        {
            lock (_tree)
            {
                _tree.InsertItem(item);
            }
        }

        public Entity RemoveItem(Entity item)
        {
            lock (_tree)
            {
                return _tree.RemoveItem(item);
            }
        }

        public Entity RemoveItem(int entityId)
        {
            lock (_tree)
            {
                return _tree.RemoveItem(entityId);
            }
        }

        public IEnumerable<Entity> RemoveItems(IEnumerable<Entity> items)
        {
            List<Entity> ret;
            lock (_tree)
            {
                ret = _tree.RemoveItems(items).ToList();
            }
            return ret;
        }

        public IEnumerable<Entity> RemoveItems(IEnumerable<int> items)
        {
            List<Entity> ret;
            lock (_tree)
            {
                ret = _tree.RemoveItems(items).ToList();
            }
            return ret;
        }

        public IEnumerable<Entity> RemoveItems(ChunkKey area)
        {
            List<Entity> ret;
            lock (_tree)
            {
                ret = _tree.RemoveItems(area).ToList();
            }
            return ret;
        }

        public IEnumerable<Entity> RemoveItemsNotInAreas(IEnumerable<ChunkKey> areas)
        {
            List<Entity> ret;
            lock (_tree)
            {
                ret = _tree.RemoveItemsNotInAreas(areas).ToList();
            }
            return ret;
        }

        public IEnumerable<Entity> GetAllItems(EntityTest test = null)
        {
            List<Entity> ret;
            lock (_tree)
            {
                ret = _tree.GetAllItems(test).ToList();
            }
            return ret;
        }

        public IEnumerable<Entity> GetItemsInArea(List<ChunkKey> keysInArea, Vector3 centre, float range, EntityTest test = null)
        {
            List<Entity> ret;
            lock (_tree)
            {
                ret = _tree.GetItemsInArea(keysInArea, centre, range, test).ToList();
            }
            return ret;
        }

        public IEnumerable<Entity> GetItemsInArea(ChunkKey area, EntityTest test = null)
        {
            List<Entity> ret;
            lock (_tree)
            {
                ret = _tree.GetItemsInArea(area, test).ToList();
            }
            return ret;
        }

        public Entity GetItem(int entityId)
        {
            Entity ret;
            lock (_tree)
            {
                ret = _tree.GetItem(entityId);
            }
            return ret;
        }

        public void AddArea(ChunkKey area)
        {
            lock (_tree)
            {
                _tree.AddArea(area);
            }
        }

        public bool ContainsArea(ChunkKey area)
        {
            lock (_tree)
            {
                return _tree.ContainsArea(area);
            }
        }
    }
}
