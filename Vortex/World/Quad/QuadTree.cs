using System.Linq;
using System.Collections.Generic;
using Psy.Core;
using SlimMath;
using Vortex.Interface.EntityBase;
using Vortex.Interface.EntityBase.Properties;
using Vortex.Interface.Traits;
using Vortex.Interface.World.Chunks;
using Vortex.World.Chunks;

namespace Vortex.World.Quad
{
    public class QuadTree : IQuadTree
    {
        private readonly Dictionary<ChunkKey, QuadTreeRegion> _regions;
        private readonly Dictionary<int, QuadTreeRegion> _idToQuad;

        public QuadTree()
        {
            _regions = new Dictionary<ChunkKey, QuadTreeRegion>();
            _idToQuad = new Dictionary<int, QuadTreeRegion>();
        }

        private void TestRefreshItem(Entity changedEntity, Trait changed)
        {
            if (changed.PropertyId == (short)EntityPropertyEnum.Position)
                UpdateItem(changedEntity);
        }

        public void UpdateItems(IEnumerable<Entity> items)
        {
            foreach (var item in items)
                UpdateItem(item);
        }

        public void UpdateItem(Entity changed)
        {
            var currentQuad = _idToQuad[changed.EntityId];
            if (currentQuad.UpdateEntity(changed))
                return;

            RemoveItem(changed);
            InsertItem(changed);
        }

        public void InsertItem(Entity item)
        {
            var area = Utils.GetChunkKeyForPosition(item.GetPosition());
            InsertItem(area, item);
        }

        public void InsertItem(ChunkKey area, Entity item)
        {
            if (_idToQuad.ContainsKey(item.EntityId))
                return;

            if (!_regions.ContainsKey(area))
                AddArea(area);

            var region = _regions[area];
            region.AddItem(item);
            _idToQuad.Add(item.EntityId, region);
            item.OnPropertyChanged += TestRefreshItem;
        }

        public Entity RemoveItem(Entity item)
        {
            return RemoveItem(item.EntityId);
        }

        public Entity RemoveItem(int entityId)
        {
            QuadTreeRegion region;
            if (_idToQuad.TryGetValue(entityId, out region))
            {
                _idToQuad.Remove(entityId);
                return region.RemoveItem(entityId);
            }
            return null;
        }

        public IEnumerable<Entity> RemoveItems(IEnumerable<Entity> items)
        {
            var ret = new List<Entity>();
            foreach (var item in items)
            {
                var retItem = RemoveItem(item);
                if (retItem != null)
                    ret.Add(retItem);
            }

            return ret;
        }

        public IEnumerable<Entity> RemoveItems(IEnumerable<int> items)
        {
            var ret = new List<Entity>();
            foreach (var item in items)
            {
                var retItem = RemoveItem(item);
                if (retItem != null)
                    ret.Add(retItem);
            }

            return ret;
        }

        public IEnumerable<Entity> RemoveItems(ChunkKey area)
        {
            if (!_regions.ContainsKey(area))
                return new List<Entity> ();

            var ret = _regions[area].GetEntities();
            _regions.Remove(area);
            return ret;
        }

        public IEnumerable<Entity> RemoveItemsNotInAreas(IEnumerable<ChunkKey> areas)
        {
            var hashAreas = new HashSet<ChunkKey>();
            foreach (var item in areas)
                hashAreas.Add(item);

            var keysToRemove = _regions.Keys.Where(item => !hashAreas.Contains(item));
            var ret = new List<Entity>();

            foreach (var key in keysToRemove)
                ret.AddRange(RemoveItems(key));

            return ret;
        }

        public IEnumerable<Entity> GetAllItems(EntityTest test=null)
        {
            var result = _regions.SelectMany(item => item.Value.GetEntities());
            if (test != null)
                return result.Where(item => test(item));
            return result;
        }

        public IEnumerable<Entity> GetItemsInArea(List<ChunkKey> keysInArea, Vector3 centre, float range, EntityTest test=null)
        {
            var regions = new List<QuadTreeRegion>(keysInArea.Count);
            regions.AddRange(from key in keysInArea where _regions.ContainsKey(key) 
                             select _regions[key]);

            var rangeSqrd = range*range;
            var result = regions.SelectMany(item => item.GetEntitiesInRegion(centre, range, rangeSqrd));
            if (test != null)
                return result.Where(item => test(item));
            return result;
        }

        public IEnumerable<Entity> GetItemsInArea(ChunkKey area, EntityTest test=null)
        {
            if (!_regions.ContainsKey(area))
                return new List<Entity>();
            
            var result = _regions[area].GetEntities();

            if (test != null)
                return result.Where(item => test(item));

            return result;
        }

        public Entity GetItem(int entityId)
        {
            if (!_idToQuad.ContainsKey(entityId))
                return null;

            var quad = _idToQuad[entityId];
            if (quad == null)
                return null;

            return quad.GetEntity(entityId);
        }

        public void AddArea(ChunkKey area)
        {
            if (_regions.ContainsKey(area))
                return;

            var bottomLeft = Utils.GetChunkWorldVectorWithOffset(area);
            var topRight = Utils.GetChunkWorldVectorWithOffset(area, new Vector3(Chunk.ChunkWorldSize, Chunk.ChunkWorldSize, 0));
            _regions.Add(area, new QuadTreeRegion(bottomLeft.AsVector2(), topRight.AsVector2()));
        }

        public bool ContainsArea(ChunkKey area)
        {
            return _regions.ContainsKey(area);
        }
    }
}
