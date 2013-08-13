using System;
using System.Linq;
using System.Collections.Generic;
using Psy.Core;
using SlimMath;
using Vortex.Interface.EntityBase;

namespace Vortex.World.Quad
{
    internal class QuadTreeRegion
    {
        private static int _nextId;
        private const int MaxItems = 5;
        private const short MaxDepth = 10;

        private readonly short _depth;
        private Dictionary<int, Entity> _items;
        private readonly Dictionary<int, QuadTreeRegion> _subAreas;
        public readonly Rectangle Area;

        public readonly int Id;


        public QuadTreeRegion(Vector2 bottomLeft, Vector2 topRight)
        {
            _items = new Dictionary<int, Entity>();
            _subAreas = new Dictionary<int, QuadTreeRegion>();
            Area = new Rectangle(new Vector2(bottomLeft.X, topRight.Y), 
                                    new Vector2(topRight.X, bottomLeft.Y));
            Id = ++_nextId;
            _depth = 0;
        }

        protected QuadTreeRegion(Vector2 bottomLeft, Vector2 topRight, short depth)
            :this(bottomLeft, topRight)
        {
            _depth = depth;
        }

        private void PerformSplit()
        {
            var height = (Area.TopLeft - Area.BottomLeft) / 2;
            var width = (Area.TopRight - Area.TopLeft)/2;

            var middlePoint = Area.BottomLeft + (Area.TopRight - Area.BottomLeft)/2;
            var topMiddlePoint = middlePoint + height;
            var bottomMiddlePoint = middlePoint - height;
            var leftMiddlePoint = middlePoint - width;
            var rightMiddlePoint = middlePoint + width;

            var newAreas = new[]
                               {
                                   new QuadTreeRegion(Area.BottomLeft, middlePoint, (short)(_depth + 1)),
                                   new QuadTreeRegion(leftMiddlePoint, topMiddlePoint, (short)(_depth + 1)),
                                   new QuadTreeRegion(bottomMiddlePoint, rightMiddlePoint, (short)(_depth + 1)),
                                   new QuadTreeRegion(middlePoint, Area.TopRight, (short)(_depth + 1))
                               };

            foreach (var item in newAreas)
            {
                _subAreas.Add(item.Id, item);
            }

            var tmp = _items;
            _items = new Dictionary<int, Entity>();
            foreach (var item in tmp.Values)
            {
                AddItem(item);
            }
        }

        private void SplitIfRequired()
        {
            if (_items.Count > MaxItems && MaxDepth < _depth)
                PerformSplit();
        }

        public void AddItem(Entity item)
        {
            var position = item.GetPosition().AsVector2();
            _items.Add(item.EntityId, item);

            if (_subAreas.Count == 0)
            {
                SplitIfRequired();
            }
            else
            {
                var subArea = _subAreas.Values.FirstOrDefault(area => area.Area.Contains(position));
                if (subArea == null)
                    throw new Exception("Quad region internal error");
                subArea.AddItem(item);
            }
        }

        public Entity RemoveItem(int entityId)
        {
            Entity ret = null;
            if (_items.ContainsKey(entityId))
            {
                ret = _items[entityId];
                _items.Remove(entityId);
            }

            if (_subAreas.Count > 0 && ret != null)
            {
                ret = _subAreas.Select(item => item.Value.RemoveItem(entityId)).FirstOrDefault(item => item != null);
            }

            return ret;
        }

        public IEnumerable<Entity> GetEntitiesInRegion(Vector3 centre, float range, float rangeSquared)
        {
            if (!RegionInArea(centre.AsVector2(), range))
                return new List<Entity>();

            if (_items.Count > 0)
            {
                return _items.Values.Where(item =>
                                               {
                                                   var dist = range + item.Radius;
                                                   return item.GetPosition().DistanceSquared(centre) < dist*dist;
                                               });
            }
            else
            {
                return _subAreas.Values.SelectMany(item => item.GetEntitiesInRegion(centre, range, rangeSquared));
            }
        }

        private bool RegionInArea(Vector2 centre, float range)
        {
            if (Area.TopLeft.Y < (centre.Y - range))
                return false;
            if (Area.BottomRight.Y > (centre.Y + range))
                return false;
            if (Area.TopLeft.X > (centre.X + range))
                return false;
            if (Area.BottomRight.X < (centre.X - range))
                return false;
            return true;
        }

        public IEnumerable<Entity> GetEntities()
        {
            return _items.Values;
        }

        public Entity GetEntity(int entityId)
        {
            if (_items.ContainsKey(entityId))
                return _items[entityId];

            return null;
        }

        // if the entity remains in the same quad, then do nothing - else return false
        public bool UpdateEntity(Entity changed)
        {
            if (!_items.ContainsKey(changed.EntityId))
                return false;

            var position = changed.GetPosition();
            var vec = new Vector2(position.X, position.Y);
            if (!Area.Contains(vec))
                return false;

            // so the item is in this quad, but if it's in a subquad then it might be 
            // in the wrong one... so to flush it out we just need to remove and readd it

            RemoveItem(changed.EntityId);
            AddItem(changed);
            return true;
        }
    }
}
