using System.Collections.Generic;
using SlimMath;
using Vortex.Interface.EntityBase;
using Vortex.Interface.World.Chunks;

namespace Vortex.World.Quad
{
    public delegate bool EntityTest(Entity entity);

    public interface IQuadTree
    {
        void UpdateItems(IEnumerable<Entity> items);
        void UpdateItem(Entity changed);

        void InsertItem(ChunkKey area, Entity item);
        void InsertItem(Entity item);

        Entity RemoveItem(Entity item);
        Entity RemoveItem(int entityId);
        IEnumerable<Entity> RemoveItems(IEnumerable<Entity> items);
        IEnumerable<Entity> RemoveItems(IEnumerable<int> items);
        IEnumerable<Entity> RemoveItems(ChunkKey area);
        IEnumerable<Entity> RemoveItemsNotInAreas(IEnumerable<ChunkKey> areas);

        IEnumerable<Entity> GetAllItems(EntityTest test=null);
        IEnumerable<Entity> GetItemsInArea(List<ChunkKey> keysInArea, Vector3 centre, float range, EntityTest test = null);
        IEnumerable<Entity> GetItemsInArea(ChunkKey area, EntityTest test = null);
        Entity GetItem(int entityId);

        void AddArea(ChunkKey area);
        bool ContainsArea(ChunkKey area);
    }
}
