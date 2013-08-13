using System.Linq;
using System.Collections.Generic;
using Psy.Core.Logging;
using SlimMath;
using Vortex.Interface.EntityBase;
using Vortex.Interface.EntityBase.Properties;
using Vortex.Interface.World.Chunks;

namespace Vortex.World.Quad
{
    public class SplitQuadTree : IQuadTree
    {
        private readonly IQuadTree _staticEntites;
        private readonly IQuadTree _mobileEntities;


        public static bool StaticEntitiesOnly(Entity item)
        {
            return (item.GetStatic());
        }

        public static bool MobileEntitiesOnly(Entity item)
        {
            return !(item.GetStatic());
        }

        public SplitQuadTree(IQuadTree statics, IQuadTree mobile)
        {
            _staticEntites = statics;
            _mobileEntities = mobile;
        }

        public void UpdateItems(IEnumerable<Entity> items)
        {
            foreach (var item in items)
                UpdateItem(item);
        }

        public void UpdateItem(Entity changed)
        {
            if (changed.GetStatic())
                _staticEntites.UpdateItem(changed);
            else
                _mobileEntities.UpdateItem(changed);
        }

        public void InsertItem(ChunkKey area, Entity item)
        {
            Logger.Write(string.Format("Adding entity: {0}, {1}", item.EntityId, item.GetStatic()));
            if (item.GetStatic())
                _staticEntites.InsertItem(area, item);
            else
                _mobileEntities.InsertItem(area, item);
        }

        public void InsertItem(Entity item)
        {
            Logger.Write(string.Format("Adding entity: {0}, {1}", item.EntityId, item.GetStatic()));
            if (item.GetStatic())
                _staticEntites.InsertItem(item);
            else
                _mobileEntities.InsertItem(item);
        }

        public Entity RemoveItem(Entity item)
        {
            if (item.GetStatic())
                return _staticEntites.RemoveItem(item);

            return _mobileEntities.RemoveItem(item);
        }

        public Entity RemoveItem(int entityId)
        {
            var ret = _staticEntites.RemoveItem(entityId);
            if (ret != null)
                return ret;
            return _mobileEntities.RemoveItem(entityId);
        }

        public IEnumerable<Entity> RemoveItems(IEnumerable<Entity> items)
        {
            var ret = new List<Entity>();

            foreach (var item in items)
            {
                if (item.GetStatic())
                    ret.Add(_staticEntites.RemoveItem(item));
                else
                    ret.Add(_mobileEntities.RemoveItem(item));
            }

            return ret;
        }

        public IEnumerable<Entity> RemoveItems(IEnumerable<int> items)
        {
            var itemList = items.ToList();
            var stat = _staticEntites.RemoveItems(itemList);
            var mob = _mobileEntities.RemoveItems(itemList);

            return stat.Concat(mob);
        }

        public IEnumerable<Entity> RemoveItems(ChunkKey area)
        {
            var stat = _staticEntites.RemoveItems(area);
            var mob = _mobileEntities.RemoveItems(area);

            return stat.Concat(mob);
        }

        public IEnumerable<Entity> RemoveItemsNotInAreas(IEnumerable<ChunkKey> areas)
        {
            var areaList = areas.ToList();
            var stat = _staticEntites.RemoveItemsNotInAreas(areaList);
            var mob = _mobileEntities.RemoveItemsNotInAreas(areaList);

            return stat.Concat(mob);
        }

        public IEnumerable<Entity> GetAllItems(EntityTest test = null)
        {
            if (test == StaticEntitiesOnly)
                return _staticEntites.GetAllItems();

            if (test == MobileEntitiesOnly)
                return _mobileEntities.GetAllItems();

            var stat = _staticEntites.GetAllItems(test);
            var mob = _mobileEntities.GetAllItems(test);

            return stat.Concat(mob);
        }

        public IEnumerable<Entity> GetItemsInArea(List<ChunkKey> keysInArea, Vector3 centre, float range, EntityTest test = null)
        {
            if (test == StaticEntitiesOnly)
                return _staticEntites.GetItemsInArea(keysInArea, centre, range);
            if (test == MobileEntitiesOnly)
                return _mobileEntities.GetItemsInArea(keysInArea, centre, range);

            var stat = _staticEntites.GetItemsInArea(keysInArea, centre, range, test);
            var mob = _mobileEntities.GetItemsInArea(keysInArea, centre, range, test);

            return stat.Concat(mob);
        }

        public IEnumerable<Entity> GetItemsInArea(ChunkKey area, EntityTest test = null)
        {
            if (test == StaticEntitiesOnly)
                return _staticEntites.GetItemsInArea(area);
            if (test == MobileEntitiesOnly)
                return _mobileEntities.GetItemsInArea(area);

            var stat = _staticEntites.GetItemsInArea(area, test);
            var mob = _mobileEntities.GetItemsInArea(area, test);

            return stat.Concat(mob);
        }

        public Entity GetItem(int entityId)
        {
            var entity = _staticEntites.GetItem(entityId);
            if (entity != null)
                return entity;

            return _mobileEntities.GetItem(entityId);
        }

        public void AddArea(ChunkKey area)
        {
            _staticEntites.AddArea(area);
            _mobileEntities.AddArea(area);
        }

        public bool ContainsArea(ChunkKey area)
        {
            return _staticEntites.ContainsArea(area) && _mobileEntities.ContainsArea(area);
        }
    }
}
