using System;
using System.Collections.Generic;
using System.Linq;
using Psy.Core;
using Psy.Core.Collision;
using SlimMath;
using Vortex.Interface;
using Vortex.Interface.Debugging;
using Vortex.Interface.World;
using Vortex.Interface.World.Chunks;
using Vortex.World.Interfaces;
using Vortex.World.Observable;
using Vortex.World.Observable.Workers;

namespace Vortex.World
{
    public class Map : IMap
    {
        private readonly IOutsideLightingColour _outsideLightingColour;
        private readonly WorldDataCache _dataCache;
        private readonly Dictionary<ICamera, IObservableArea> _observableAreas;
        private readonly Dictionary<ChunkKey, IMeshCollisionTester> _staticEntityMeshes;
        private BottomLeftUpdateWorker _bottomLeftUpdateWorker;
        private LightsUpdateWorker _lightsUpdateWorker;
        private MeshUpdateWorker _meshUpdateWorker;
        private MeshCalculatorWorker _meshCalculatorWorker;

        public float MaximumObservableAreaSize { get; private set; }

        public string TileSetName { get; set; }

        public Map(IOutsideLightingColour outsideLightingColour, WorldDataCache cache)
        {
            // todo: consider moving this elsewhere? a config maybe?
            TileSetName = "tileset1.adf";
            _outsideLightingColour = outsideLightingColour;
            _dataCache = cache;
            _observableAreas = new Dictionary<ICamera, IObservableArea>();
            _staticEntityMeshes = new Dictionary<ChunkKey, IMeshCollisionTester>();

            MaximumObservableAreaSize = 0;
        }

        public void Dispose()
        {
            foreach (var observableArea in _observableAreas)
            {
                observableArea.Value.Dispose();
            }
        }

        public void SetStaticItemsInChunk(ChunkKey key, IEnumerable<IMeshCollisionTester> items)
        {
            _staticEntityMeshes.Remove(key);
            var tester = new MultiMeshCollisionTester(items);
            _staticEntityMeshes.Add(key, tester);

            foreach (var observedArea in _observableAreas.Values)
            {
                if (observedArea.ChunksObserved.SelectMany(item => item).Contains(key))
                    UpdateObservableArea(observedArea);
            }
        }

        private void UpdateObservableArea(IObservableArea area)
        {
            area.ForceUpdate();
        }

        public bool IsLineOfSight(Vector3 @from, Vector3 to)
        {
            // test the map collision - then ensure that if there is a collision, that it's further away from 'To'
            var direction = to - from;
            var result = TestMapCollision(from, direction);

            if (!result.HasCollided)
                return true;
            if (result.CollisionPoint.Distance(from) > to.Distance(from))
                return true;
            return false;
        }

        // include static entities and not just the terrain
        public CollisionResult TestMapCollision(Vector3 @from, Vector3 direction)
        {
            return TestMapCollision(from, direction, null);
        }

        // includes static entities and not just the terrain by default
        public CollisionResult TestMapCollision(Vector3 @from, Vector3 direction, IEnumerable<Mesh> additionalTargets)
        {
            // get the meshes of all the observable areas - mix them all in together
            // then throw in the additional Targets and return the result...
            var tester = new MultiMeshCollisionTester();

            if (additionalTargets != null)
            {
                tester.AddRange(additionalTargets.Where(target => target != null).Select(target => new MeshCollisionTester(target)));
            }

            // TODO - if the from & to aren't in the same observable area then return false ...
            // TODO - then only need to use the mesh from the 1 observeable area
            foreach(var area in _observableAreas.Values)
            {
                tester.Add(area.CollisionTester);
            }

            return tester.CollideWithRay(from, direction).RayCollisionResult;
        }

        public IObservableArea AddCamera(ICamera camera)
        {
            if (_observableAreas.ContainsKey(camera))
                return _observableAreas[camera];

            _bottomLeftUpdateWorker = new BottomLeftUpdateWorker();
            _lightsUpdateWorker = new LightsUpdateWorker(_dataCache);
            _meshUpdateWorker = new MeshUpdateWorker(_dataCache);
            _meshCalculatorWorker = new MeshCalculatorWorker();

            var updateMethods = 
                new List<IObservableAreaWorker>
                {
                    _bottomLeftUpdateWorker,
                    _lightsUpdateWorker,
                    _meshUpdateWorker,
                    _meshCalculatorWorker,
                };

            var observableArea = new ObservableArea(_outsideLightingColour, () => camera.Vector, _dataCache, updateMethods);
            _observableAreas.Add(camera, observableArea);

            MaximumObservableAreaSize = Math.Max(MaximumObservableAreaSize, observableArea.ObservedSize);

            return observableArea;
        }

        public void RemoveCamera(ICamera camera)
        {
            if (!_observableAreas.ContainsKey(camera))
            {
                throw new ApplicationException("Camera does not exist in the map");
            }
            var observableArea = _observableAreas[camera];
            _observableAreas.Remove(camera);
            observableArea.Dispose();
        }

        public TimingStats Update()
        {
            var ret = new TimingStats("Map Updating");
            var count = 0;

            foreach (var observableArea in _observableAreas.Values)
            {
                var name = string.Format("Updating OA {0}", ++count);
                ret.StartingTask(name);
                observableArea.Update();
                observableArea.SwapBuffers();
                ret.CompletedTask(name);
            }

            var triggers = GetObservedChunkKeys().SelectMany(item => _dataCache.GetTriggers(item));

            foreach (var trigger in triggers)
            {
                var name = string.Format("Updating trigger {0}:{1}", trigger.UniqueKey.ChunkLocation, trigger.UniqueKey.Id);
                ret.StartingTask(name);
                trigger.Update();
                ret.CompletedTask(name);
            }

            return ret;
        }

        public HashSet<ChunkKey> AllObservedChunks()
        {
            var ret = new HashSet<ChunkKey>();

            foreach (var item in _observableAreas.Select(item => item.Value).SelectMany(area => area.ChunksObservedExtended))
            {
                ret.Add(item);
            }

            return ret;
        }

        public IEnumerable<ChunkKey> GetObservedChunkKeys()
        {
            var ret = new HashSet<ChunkKey>();
            foreach (var item in _observableAreas)
            {
                foreach (var next in item.Value.ChunksObserved.SelectMany(items => items))
                    ret.Add(next);
            }
            return ret;
        }
    }
}
