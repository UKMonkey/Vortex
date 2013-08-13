using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimMath;
using Vortex.Interface.EntityBase;
using Vortex.Interface.World.Chunks;
using Vortex.World.Chunks;

namespace Vortex.World.Quad
{
    public class ShitQuadTree : IQuadTree
    {
        private readonly Dictionary<int, Entity> _entities = new Dictionary<int, Entity>();

        public void UpdateItems(IEnumerable<Entity> items)
        {
            foreach (var item in items)
                UpdateItem(item);
        }

        public void UpdateItem(Entity changed)
        {
            _entities[changed.EntityId] = changed;
        }

        public void InsertItem(ChunkKey area, Entity item)
        {
            _entities[item.EntityId] = item;
        }

        public void InsertItem(Entity item)
        {
            _entities[item.EntityId] = item;
        }

        public Entity RemoveItem(Entity item)
        {
            var tmp = _entities[item.EntityId];
            _entities.Remove(item.EntityId);
            return tmp;
        }

        public Entity RemoveItem(int entityId)
        {
            var tmp = _entities[entityId];
            _entities.Remove(entityId);
            return tmp;
        }

        public IEnumerable<Entity> RemoveItems(IEnumerable<Entity> items)
        {
            return items.Select(RemoveItem);
        }

        public IEnumerable<Entity> RemoveItems(IEnumerable<int> items)
        {
            return items.Select(RemoveItem);
        }

        public IEnumerable<Entity> RemoveItems(ChunkKey area)
        {
            var entities = GetItemsInArea(area);
            return RemoveItems(entities);
        }

        public IEnumerable<Entity> RemoveItemsNotInAreas(IEnumerable<ChunkKey> areas)
        {
            return new List<Entity>();
        }

        public IEnumerable<Entity> GetAllItems(EntityTest test = null)
        {
            return _entities.Values;
        }

        public IEnumerable<Entity> GetItemsInArea(List<ChunkKey> keysInArea, Vector3 centre, float range, EntityTest test = null)
        {
            var tmp = _entities.Values.Where(item =>((item.GetPosition() - centre).Length < range));
            if (test != null)
                return tmp.Where(item => test(item));
            return tmp;
        }

        public IEnumerable<Entity> GetItemsInArea(ChunkKey area, EntityTest test = null)
        {
            return 
                (from item in _entities.Values 
                 let areaA = Utils.GetChunkKeyForPosition(item.GetPosition()) 
                 where areaA == area select item);
        }

        public Entity GetItem(int entityId)
        {
            if (_entities.ContainsKey(entityId))
                return _entities[entityId];
            return null;
        }

        public void AddArea(ChunkKey area)
        {
        }

        public bool ContainsArea(ChunkKey area)
        {
            return true;
        }
    }
}
