using System;
using System.Collections.Generic;
using System.Linq;
using SlimMath;
using Vortex.Interface.EntityBase;

namespace Vortex.World.EntityMonitor
{
    public class TestCache
    {
        private delegate bool AreaTest(Vector3 testPosition, Entity item);

        private short _testCount;
        private readonly Entity _target;
        private readonly List<AreaTest> _testInArea;
        private readonly EntityTester _isVisible;

        private readonly EntityHandler _onVisible;
        private readonly EntityHandler _onHidden;

        public TestCache(Entity target, EntityTester isVisible, EntityHandler onVisible, EntityHandler onHidden)
        {
            _testInArea = new List<AreaTest>
                              {
                                  EntityInQuadA,
                                  EntityInQuadB,
                                  EntityInQuadC,
                                  EntityInQuadD
                              };
            _target = target;
            _isVisible = isVisible;

            _onVisible = onVisible;
            _onHidden = onHidden;
        }

        public void UpdateCache(IEnumerable<Entity> allEntities)
        {
            var nextIndex = GetNextSectionIndex();
            var test = _testInArea[nextIndex];
            var position = _target.GetPosition();

            var observedEntities = allEntities.
                Where(entity => test(position, entity)).ToList();

            var nonVisible = new List<Entity>(observedEntities.Count);
            var visible = new List<Entity>(observedEntities.Count);

            foreach (var entity in observedEntities)
            {
                if (_isVisible(_target, entity, observedEntities))
                    visible.Add(entity);
                else
                    nonVisible.Add(entity);
            }

            _onVisible(visible);
            _onHidden(nonVisible);

            _testCount++;
        }

        private int GetNextSectionIndex()
        {
            return (Math.Abs(_testCount % _testInArea.Count));
        }

        private static bool EntityInQuadA(Vector3 eyes, Entity item)
        {
            return EntityInQuad(eyes, item, true, true);
        }
        private static bool EntityInQuadB(Vector3 eyes, Entity item)
        {
            return EntityInQuad(eyes, item, true, false);
        }
        private static bool EntityInQuadC(Vector3 eyes, Entity item)
        {
            return EntityInQuad(eyes, item, false, true);
        }
        private static bool EntityInQuadD(Vector3 eyes, Entity item)
        {
            return EntityInQuad(eyes, item, false, false);
        }

        private static bool EntityInQuad(Vector3 eyes, Entity item, bool requirePositiveX, bool requirePositiveY)
        {
            var position = item.GetPosition();
            var xDifference = position.X - eyes.X;
            var yDifference = position.Y - eyes.Y;

            if (!requirePositiveX)
                xDifference *= -1;
            if (!requirePositiveY)
                yDifference *= -1;

            if (xDifference < 0)
                return false;

            if (yDifference < 0)
                return false;

            return true;
        }
    }
}
