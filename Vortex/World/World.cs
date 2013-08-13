using System;
using System.Collections.Generic;
using System.Linq;
using SlimMath;
using Vortex.BulletTracer;
using Vortex.Interface.EntityBase;
using Vortex.Interface.EntityBase.Behaviours;
using Vortex.Interface.EntityBase.Properties;
using Vortex.Interface.World;
using Vortex.Interface.World.Chunks;
using Vortex.World.Chunks;
using Psy.Core;
using Psy.Core.Collision;
using Vortex.Interface.Debugging;
using Vortex.World.EntityMonitor;
using Vortex.World.Interfaces;
using Vortex.World.Movement;


namespace Vortex.World
{
    public class World : IOutsideLightingColour, ITimeOfDayProvider
    {
        public uint TimeOfDay { get; set; }

        public uint TicksPerDay
        {
            get { return 1000; }
        }

        private readonly WorldDataCache _cache;
        private readonly IMovementHandler _movementHandler;
        private readonly Dictionary<Entity, TestCache> _visableEntities;

        private IMap _map;

        public bool IsRaining { get; set; }
        public Color4 OutsideLightingColour { get; set; }

        public BulletCollection Bullets { get; private set; }

        public World(WorldDataCache cache, IMovementHandler movementHandler)
        {
            TimeOfDay = 0;
            OutsideLightingColour = new Color4(1.0f, 0.8f, 0.8f, 0.9f);
            Bullets = new BulletCollection();

            _movementHandler = movementHandler;
            _movementHandler.World = this;

            _cache = cache;
            _cache.OnEntitiesLoaded += EntitiesLoaded;
            _cache.OnEntitiesDeleted += EntitiesDeleted;
            _cache.OnEntitiesUpdated += EntitiesUpdated;

            _visableEntities = new Dictionary<Entity, TestCache>();
        }

        public void Dispose()
        {
            if (_map != null)
                _map.Dispose();
        }

        public void AddEntities(List<Entity> entities)
        {
            _cache.AddEntities(entities);
        }

        public void AddEntity(Entity newEntity)
        {
            _cache.AddEntities(new List<Entity> { newEntity });
        }

        private void EntitiesLoaded(List<Entity> entities)
        {
            UpdateObservableAreaMeshIfNeeded(entities);
        }

        private void EntitiesUpdated(List<Entity> entities)
        {
            UpdateObservableAreaMeshIfNeeded(entities);
        }

        private void EntitiesDeleted(List<Entity> entities)
        {
            UpdateObservableAreaMeshIfNeeded(entities);
            foreach (var entity in entities.Where(item => _visableEntities.ContainsKey(item)))
            {
                _visableEntities.Remove(entity);
            }
        }

        public ICollection<Entity> GetObservedEntities()
        {
            return _cache.GetObservedEntities();
        }

        private void UpdateObservableAreaMeshIfNeeded(IEnumerable<Entity> entities)
        {
            foreach (var entity in entities)
            {
                if (!entity.GetStatic()) 
                    continue;

                var chunkKey = Utils.GetChunkKeyForPosition(entity.GetPosition());
                var allTesters = _cache.GetStaticEntities(chunkKey).Where(item => item.Mesh != null).Select(item => new MeshCollisionTester(item.Mesh));
                _map.SetStaticItemsInChunk(chunkKey, allTesters);
                return;
            }
        }

        public void UseMap(IMap map)
        {
            _map = map;
        }

        public IMap GetMap()
        {
            return _map;
        }

        public TimingStats Update()
        {
            var ret = new TimingStats("World.Update");

            if (_map == null)
                return ret;

            ret.StartingTask("Updating bullets");
            Bullets.Update();
            ret.CompletedTask("Updating bullets");

            ret.StartingTask("Updating map");
            ret.MergeStats(_map.Update());
            ret.CompletedTask("Updating map");
            
            ret.StartingTask("Updating entities");
            UpdateEntities();
            ret.CompletedTask("Updating entities");

            ret.StartingTask("Updating visible entities");
            foreach (var target in _visableEntities.Values)
                target.UpdateCache(GetObservedEntities());
            ret.CompletedTask("Updating visible entities");
            TimeOfDay = (++TimeOfDay) % TicksPerDay;

            return ret;
        }

        public void RegisterEntityViewSystem(Entity target, EntityTester viewTester, EntityHandler onVisible, EntityHandler onHidden)
        {
            _visableEntities[target] = new TestCache(target, viewTester, onVisible, onHidden);
        }

        public List<Entity> GetObservedMobileEntities()
        {
            var chunkKeys = _map.GetObservedChunkKeys();
            var ret = new List<Entity>();

            foreach (var key in chunkKeys)
            {
                ret.AddRange(_cache.GetMobileEntities(key));
            }

            return ret;
        }

        public List<Entity> GetObservedstaticEntities()
        {
            var chunkKeys = _map.GetObservedChunkKeys();
            var ret = new List<Entity>();

            foreach (var key in chunkKeys)
            {
                ret.AddRange(_cache.GetStaticEntities(key));
            }

            return ret;
        }

        private void TestCollisionWithEntities(Entity entity)
        {
            if (entity.PendingDestruction)
                return;

            var entities = _cache.GetEntitiesInRange(entity.GetPosition(), entity.Radius);

            foreach (var otherEntity in entities)
            {
                if (otherEntity.PendingDestruction)
                    continue;

                if (entity.GetStatic() && otherEntity.GetStatic())
                    continue;

                if (otherEntity.EntityId == entity.EntityId)
                    continue;

                if (!entity.CollidesWith(otherEntity))
                    continue;

                entity.PerformBehaviour(EntityBehaviourEnum.OnCollisionWithEntity, otherEntity);
                otherEntity.PerformBehaviour(EntityBehaviourEnum.OnCollisionWithEntity, entity);
            }
        }

        private void UpdateEntities()
        {
            var entities = GetObservedEntities().ToList();
            var entitiesToUpdate = new List<Entity>();

            /** it is expected that any think implmentation will
             *  NOT UPDATE ANY ENTITIES OTHER THAN THE ONE BEING EXAMINED
             *  which is how this is kept thread safe.
             *  otherwise there's probably an explosion waiting to happen
             */
            foreach (var entity in entities)
            {
                if (!entity.PendingDestruction)
                {
                    entity.Think();
                    entity.UpdateAnimation();
                }
            }

            foreach (var entity in entities)
            {
                var moving = _movementHandler.IsEntityMoving(entity);
                if (!moving)
                    continue;

                if (!entity.PendingDestruction)
                {
                    if (_movementHandler.HandleEntityMovement(entity))
                        entitiesToUpdate.Add(entity);
                }

                TestCollisionWithEntities(entity);
            }

            _cache.UpdateEntities(entitiesToUpdate);
        }

        public IEnumerable<Entity> GetEntitiesWithinArea(Vector3 centre, float distance)
        {
            return _cache.GetEntitiesInRange(centre, distance);
        }

        public CollisionResult TraceRay(Vector3 source, Vector3 direction)
        {
            return TraceRay(source, direction, (Func<Entity, bool>) null);
        }

        public CollisionResult TraceRay(Vector3 source, Vector3 direction, Func<Entity, bool> filter)
        {
            var entities = filter == null ? new List<Entity>() : GetObservedEntities().Where(filter);
            return TraceRay(source, direction, entities);
        }

        public CollisionResult TraceRay(Vector3 source, Vector3 direction, IEnumerable<Entity> entities)
        {
            var entityMeshes = entities.Select(e => e.Mesh);
            var collisionPoint = _map.TestMapCollision(source, direction.NormalizeRet(), entityMeshes);

            if (!collisionPoint.HasCollided)
            {
                collisionPoint.CollisionPoint = source + direction * _map.MaximumObservableAreaSize;
                collisionPoint.CollisionMesh = null;
            }

            return collisionPoint;
        }

        public Entity GetEntity(int id)
        {
            return _cache.GetEntity(id);
        }

        public IEnumerable<Entity> GetEntitiesInChunk(ChunkKey area)
        {
            return _cache.GetEntities(area);
        }
    }
}
