using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SlimMath;
using Psy.Core.Logging;
using Vortex.Interface.EntityBase;
using Vortex.Interface.Traits;
using Vortex.Interface.World.Chunks;
using Vortex.Interface.World.Entities;
using Vortex.Interface.World.Triggers;
using Vortex.Interface.World.Wrapper;
using Vortex.World.Chunks;
using Vortex.World.Quad;


namespace Vortex.World
{
    public delegate IEnumerable<ChunkKey> ObservedChunkKeyDelegate();
 
    public class WorldDataCache: IChunkCache, ITriggerCache, IEntityCache
    {
        private const string LogLocation = "WorldDataCache";
        private readonly ObservedChunkKeyDelegate _observedChunkKeys;

        /** Loading / Saving methods
         */
        private readonly IWorldProvider _loader;
        private readonly IWorldSaver _saver;


        /** Entity cache info
         */
        public event EntitiesCallback OnEntitiesLoaded;
        public event EntitiesCallback OnEntitiesUpdated;
        public event EntitiesCallback OnEntitiesDeleted;

        private readonly IQuadTree _entityQuadTree;

        private List<Entity> _allObservedEntities;
        private SpinLock _allEntitiesLock;

        private readonly HashSet<Entity> _deletedEntities;
        private readonly HashSet<Entity> _updatedEntities;

       
        /** Chunks cache info
         */
        public event ChunkCallback OnChunksLoaded;
        public event ChunkCallback OnChunksUpdated;

        private readonly Dictionary<ChunkKey, Chunk> _chunkCache;

        private readonly HashSet<ChunkKey> _requestedChunks;
        private readonly List<Chunk> _updatedChunks;
        private readonly List<Chunk> _loadedChunks;


        /** Trigger cache info
         */
        public event TriggerCallback OnTriggerLoaded;
        public event TriggerCallback OnTriggerUpdated;

        private readonly Dictionary<TriggerKey, ITrigger> _triggerCache;
        private readonly Dictionary<ChunkKey, HashSet<TriggerKey>> _chunkToTriggers;

        private readonly HashSet<ChunkKey> _requestedTriggers;
        private readonly List<ITrigger> _loadedTriggers;
        private readonly List<ITrigger> _updatedTriggers; 


        public WorldDataCache(IWorldProvider worldProvider, IWorldSaver worldSaver,
                                ObservedChunkKeyDelegate observedChunkKeys)
        {
            _observedChunkKeys = observedChunkKeys;
            _allEntitiesLock = new SpinLock(false);

            _loader = worldProvider;
            _saver = worldSaver;

            // entities
            _entityQuadTree =
                new LockedQuadTree(new SplitQuadTree(new QuadTree(), new QuadTree()));

            _deletedEntities = new HashSet<Entity>();
            _updatedEntities = new HashSet<Entity>();
            _allObservedEntities = new List<Entity>();

            _loader.OnEntityGenerated += EntitiesGenerated;
            _loader.OnEntityLoaded += EntitiesLoaded;
            _loader.OnEntityUpdated += EntitiesUpdated;
            _loader.OnEntityDeleted += EntitiesDeleted;

            // chunks
            _chunkCache = new Dictionary<ChunkKey, Chunk>();
            _requestedChunks = new HashSet<ChunkKey>();
            _updatedChunks = new List<Chunk>();
            _loadedChunks = new List<Chunk>();

            _loader.OnChunkLoad += ChunksLoaded;
            _loader.OnChunksGenerated += ChunksGenerated;

            // triggers
            _triggerCache = new Dictionary<TriggerKey, ITrigger>();
            _chunkToTriggers = new Dictionary<ChunkKey, HashSet<TriggerKey>>();
            _requestedTriggers = new HashSet<ChunkKey>();
            _loadedTriggers = new List<ITrigger>();
            _updatedTriggers = new List<ITrigger>();

            _loader.OnTriggerLoaded += TriggerLoaded;
            _loader.OnTriggerGenerated += TriggerGenerated;
        }

        public void Dispose()
        {
            _loader.Dispose();
        }

        /** Get any chunks in memory
         *  if it's not available then request it from the chunkLoader
         */
        public List<Chunk> GetChunks(IEnumerable<ChunkKey> keys)
        {
            var toLoad = new List<ChunkKey>();
            var available = new List<Chunk>();

            lock (_chunkCache)
            {
                foreach (var key in keys)
                {
                    if (_chunkCache.ContainsKey(key))
                    {
                        Logger.Write(string.Format("Chunk {0},{1} already in cache", key.X, key.Y), LoggerLevel.Trace, LogLocation);
                        available.Add(_chunkCache[key]);
                    }
                    else
                    {
                        if (!_requestedChunks.Contains(key))
                        {
                            Logger.Write(string.Format("Attempting to obtain chunk {0},{1}", key.X, key.Y), LoggerLevel.Trace, LogLocation);
                            toLoad.Add(key);
                            _requestedChunks.Add(key);
                        }
                    }
                }
            }

            if (toLoad.Count > 0)
            {
                _loader.LoadChunks(toLoad);
            }

            return available;
        }

        /** Notify the chunk chunkLoader to save chunks
         *  Register everything changed and we'll deal with it later
         */
        public void UpdateChunks(List<Chunk> changedChunks)
        {
            if (changedChunks.Count == 0)
            {
                return;
            }

            lock (_chunkCache)
            {
                _updatedChunks.InsertRange(_updatedChunks.Count, changedChunks);
            }
            _saver.SaveChunks(changedChunks);
        }

        private void ChunksGenerated(List<Chunk> chunks)
        {
            foreach (var changedChunk in chunks)
            {
                changedChunk.ChunkMesh.Compress();
                Logger.Write(string.Format("Chunk {0},{1} generated", changedChunk.Key.X, changedChunk.Key.Y), LoggerLevel.Trace, LogLocation);
            }
             
            _saver.SaveChunks(chunks);
            ChunksLoaded(chunks);
        }

        /** Called by the chunkLoader when chunks have been loaded
         *  Do nothing - deal with it later
         */
        private void ChunksLoaded(List<Chunk> chunks)
        {
            lock (_chunkCache)
            {
                _loadedChunks.AddRange(chunks);
            }

            foreach (var key in chunks.Select(item => item.Key))
            {
                _loader.LoadTriggers(key);
                _loader.LoadEntities(key);
            }
        }

        public List<ITrigger> GetTriggers(ChunkKey area)
        {
            var ret = new List<ITrigger>();
            
            lock (_triggerCache)
            {
                HashSet<TriggerKey> data;
                if (_chunkToTriggers.TryGetValue(area, out data))
                {
                    foreach (var item in data)
                    {
                        ITrigger result;
                        if (_triggerCache.TryGetValue(item, out result))
                            ret.Add(result);
                    }
                }
                else
                {
                    if (!_requestedTriggers.Contains(area))
                    {
                        _requestedTriggers.Add(area);
                        _loader.LoadTriggers(area);
                    }
                }
            }

            return ret;
        }

        public void UpdateTriggers(List<ITrigger> toUpdate)
        {
            foreach (var trigger in toUpdate)
            {
                Logger.Write(string.Format("Trigger {0},{1},{2} updated", 
                    trigger.UniqueKey.Id, trigger.UniqueKey.ChunkLocation.X, trigger.UniqueKey.ChunkLocation.Y), LoggerLevel.Trace, LogLocation);
            }

            _saver.SaveTrigger(toUpdate);
            lock (_triggerCache)
            {
                _updatedTriggers.AddRange(toUpdate);
            }
        }

        private void TriggerGenerated(ChunkKey key, List<ITrigger> triggers)
        {
            _saver.SaveTrigger(triggers);
            TriggerLoaded(key, triggers);
        }

        private void TriggerLoaded(ChunkKey key, List<ITrigger> triggers)
        {
            foreach (var trigger in triggers)
            {
                Logger.Write(string.Format("Trigger {0},{1},{2} loaded",
                    trigger.UniqueKey.Id, trigger.UniqueKey.ChunkLocation.X, trigger.UniqueKey.ChunkLocation.Y), LoggerLevel.Trace, LogLocation);
            }

            lock (_triggerCache)
            {
                _loadedTriggers.AddRange(triggers);
            }
        }

        public Entity GetEntity(int id)
        {
            return _entityQuadTree.GetItem(id);
        }

        private bool IsObserved(ChunkKey area)
        {
            return _chunkCache.ContainsKey(area);
        }

        private void LoadEntities(ChunkKey area)
        {
            _entityQuadTree.AddArea(area);
            _loader.LoadEntities(area);
        }

        public ICollection<Entity> GetObservedEntities()
        {
            try
            {
                var taken = false;
                _allEntitiesLock.Enter(ref taken);
                return _allObservedEntities;
            }
            finally
            {
                _allEntitiesLock.Exit();
            }
        }

        private List<ChunkKey> GetChunkKeysInArea(Vector3 centre, float distance)
        {
            var right = centre + new Vector3(distance, 0, 0);
            var left = centre + new Vector3(-distance, 0, 0);
            var top = centre + new Vector3(0, distance, 0);
            var bottom = centre + new Vector3(0, -distance, 0);

            var rightChunk = Utils.GetChunkKeyForPosition(right);
            var topChunk = Utils.GetChunkKeyForPosition(top);
            var bottomChunk = Utils.GetChunkKeyForPosition(bottom);
            var leftChunk = Utils.GetChunkKeyForPosition(left);

            var ret = new List<ChunkKey>((topChunk.Y - bottomChunk.Y) * (rightChunk.X - leftChunk.X));
            for (var j = bottomChunk.Y; j <= topChunk.Y; ++j)
            {
                for (var i = leftChunk.X; i <= rightChunk.X; ++i)
                {
                    ret.Add(new ChunkKey(i, j));
                }
            }

            return ret;
        }

        public IEnumerable<Entity> GetEntitiesInRange(Vector3 centre, float range)
        {
            var chunkKeys = GetChunkKeysInArea(centre, range);
            return _entityQuadTree.GetItemsInArea(chunkKeys, centre, range);
        }

        public IEnumerable<Entity> GetEntities(ChunkKey area)
        {
            if (!IsObserved(area))
                return new List<Entity>();

            if (_entityQuadTree.ContainsArea(area))
                return _entityQuadTree.GetItemsInArea(area);

            LoadEntities(area);
            return new List<Entity>();
        }

        /**
         * Debugging or console use only!
         */
        public IEnumerable<Entity> GetEntities()
        {
            return _entityQuadTree.GetAllItems();
        }

        public IEnumerable<Entity> GetStaticEntities(ChunkKey area)
        {
            if (!IsObserved(area))
                return new List<Entity>();

            if (!_entityQuadTree.ContainsArea(area))
            {
                LoadEntities(area);
                return new List<Entity>();
            }

            return _entityQuadTree.GetItemsInArea(area, SplitQuadTree.StaticEntitiesOnly);
        }

        public IEnumerable<Entity> GetMobileEntities(ChunkKey area)
        {
            if (!IsObserved(area))
                return new List<Entity>();

            if (!_entityQuadTree.ContainsArea(area))
            {
                LoadEntities(area);
                return new List<Entity>();
            }

            return _entityQuadTree.GetItemsInArea(area, SplitQuadTree.MobileEntitiesOnly);
        }

        public void SaveEntities(ICollection<Entity> entities)
        {
            if (entities.Count > 0)
                _saver.SaveEntities(entities);
        }

        private void EntitiesDeleted(List<int> toDelete)
        {
            var entitiesDeleted = DeleteExistingEntities(toDelete);

            UpdateListOfEntities();
            OnEntitiesDeleted(entitiesDeleted);
        }

        private List<Entity> DeleteExistingEntities(List<int> entities)
        {
            var ret = _entityQuadTree.RemoveItems(entities).ToList();

            lock (_updatedEntities)
            {
                foreach (var entity in ret)
                {
                    entity.Destroy();
                    _deletedEntities.Add(entity);
                    _saver.DeleteEntity(entity);
                }
            }

            return ret;
        }

        public void DeleteEntity(int toDelete)
        {
            EntitiesDeleted(new List<int> { toDelete });
        }

        private void EntitiesUpdated(List<Entity> entities)
        {
            UpdateEntities(entities);
        }

        public void UpdateEntities(List<Entity> entities)
        {
            lock (_updatedEntities)
            {
                foreach (var entity in entities.Where(item => !item.PendingDestruction))
                {
                    _updatedEntities.Add(entity);
                }
            }
        }

        public void UpdateEntity(Entity entity, Trait changed)
        {
            UpdateEntities(new List<Entity> {entity});
        }

        protected virtual void HandleNewEntities(List<Entity> entities)
        {
        }

        private void EntitiesGenerated(List<Entity> entities, ChunkKey area)
        {
            if (entities.Count == 0)
                return;

            // do the spawning before we attempt to process or save...
            HandleNewEntities(entities);

            lock (_updatedEntities)
            {
                foreach (var item in entities)
                    _updatedEntities.Add(item);
            }
        }

        public void AddEntities(List<Entity> entities)
        {
            // because the chunkkey isn't used in AddEntities
            // just pass in a rubbish on
            EntitiesGenerated(entities, new ChunkKey(0, 0));
        }

        private void EntitiesLoaded(List<Entity> entities, ChunkKey area)
        {
            lock (_updatedEntities)
            {
                foreach (var entity in entities)
                {
                    _updatedEntities.Add(entity);
                }
            }
        }

        /******************************************/

        /** Deal with anything that's been loaded or updated
         */
        public void ProcessLoadedData()
        {
            ProcessChunkData();
            ProcessTriggerData();
            ProcessEntityData();

            UpdateListOfEntities();
        }

        protected virtual void ProcessChunkData()
        {
            lock (_chunkCache)
            {
                ChunksUpdated(_updatedChunks, false);
                ChunksUpdated(_loadedChunks, true);
            }
        }

        protected virtual void ProcessTriggerData()
        {
            lock (_triggerCache)
            {
                TriggersUpdated(_updatedTriggers, false);
                TriggersUpdated(_loadedTriggers, true);
            }
        }


        protected virtual void ProcessEntityData()
        {
            List<Entity> changedEntities;

            lock (_updatedEntities)
            {
                if (_deletedEntities.Count != 0 && OnEntitiesDeleted != null)
                    OnEntitiesDeleted(_deletedEntities.ToList());
                _deletedEntities.Clear();

                changedEntities = new List<Entity>(_updatedEntities);
                _updatedEntities.Clear();
            }

            ProcessChangedEntities(changedEntities);
        }


        /** sends notifications to 'OnChunksLoaded' or 'OnChunksUpdated' for anything changed since last call.
         */
        private void ChunksUpdated(List<Chunk> changedChunks, bool isNew)
        {
            if (changedChunks.Count == 0)
                return;

            foreach (var chunk in changedChunks)
            {
                _chunkCache[chunk.Key] = chunk;
            }

            if (isNew)
            {
                if (OnChunksLoaded != null)
                    OnChunksLoaded(changedChunks);
            }
            else
            {
                if (OnChunksUpdated != null)
                    OnChunksUpdated(changedChunks);
            }

            foreach (var item in changedChunks)
            {
                _requestedChunks.Remove(item.Key);
            }

            changedChunks.Clear();
        }

        /** wrapper for the one below
         *  not ideal - but shouldn't exactly be called often
         */
        private void TriggersUpdated(List<ITrigger> changedTriggers, bool isNew)
        {
            var tmpDic = new Dictionary<ChunkKey, List<ITrigger>>();
            
            foreach(var trigger in changedTriggers)
            {
                if (!tmpDic.ContainsKey(trigger.UniqueKey.ChunkLocation))
                    tmpDic.Add(trigger.UniqueKey.ChunkLocation, new List<ITrigger>{trigger});
                else
                    tmpDic[trigger.UniqueKey.ChunkLocation].Add(trigger);
            }

            foreach (var item in tmpDic)
            {
                TriggersUpdated(item.Key, item.Value, isNew);
            }
            changedTriggers.Clear();
        }

        /** sends notifications to 'OnTriggersLoaded' or 'OnTriggersUpdated' for anything changed since last call.
         */
        private void TriggersUpdated(ChunkKey key, List<ITrigger> changedTriggers, bool isNew)
        {
            if (changedTriggers.Count == 0)
                return;
            
            foreach (var triggerKey in changedTriggers.Select(trigger => trigger.UniqueKey))
            {
                if (!_chunkToTriggers.ContainsKey(triggerKey.ChunkLocation))
                    _chunkToTriggers.Add(triggerKey.ChunkLocation, new HashSet<TriggerKey>());
                _chunkToTriggers[triggerKey.ChunkLocation].Add(triggerKey);
            }

            foreach (var trigger in changedTriggers)
            {
                if (_triggerCache.ContainsKey(trigger.UniqueKey))
                    _triggerCache.Remove(trigger.UniqueKey);
                _triggerCache.Add(trigger.UniqueKey, trigger);
            }

            if (isNew)
            {
                if (OnTriggerLoaded != null)
                    OnTriggerLoaded(key, changedTriggers);
            }
            else
            {
                if (OnTriggerUpdated != null)
                    OnTriggerUpdated(key, changedTriggers);
            }

            foreach (var item in changedTriggers)
            {
                _requestedTriggers.Remove(item.UniqueKey.ChunkLocation);
            }

            changedTriggers.Clear();
        }

        private void OnEntityDeath(Entity item)
        {
            DeleteEntity(item.EntityId);
        }

        private Entity UpdateEntityMaps(Entity entity, out bool previouslyExisting)
        {
            var previousEntity = _entityQuadTree.GetItem(entity.EntityId);
            previouslyExisting = previousEntity != null;

            if (!previouslyExisting)
                return UpdateMapWithNewEntity(entity);

            if (!ReferenceEquals(entity, previousEntity))
                previousEntity.SetProperties(entity.NonDefaultProperties);

            return previousEntity;
        }

        private Entity UpdateMapWithNewEntity(Entity entity)
        {
            var key = Utils.GetChunkKeyForPosition(entity.GetPosition());

            entity.OnDeath += OnEntityDeath;
            entity.OnPropertyChanged += UpdateEntity;
            entity.Registered = true;

            _entityQuadTree.AddArea(key);
            _entityQuadTree.InsertItem(key, entity);
            
            return entity;
        }

        private void UpdateListOfEntities()
        {
            var tmp = new List<Entity>();
            var keys = _observedChunkKeys().ToList();
            foreach (var chunkKey in keys)
            {
                tmp.AddRange(GetEntities(chunkKey));
            }

            try
            {
                var taken = false;
                _allEntitiesLock.Enter(ref taken);
                _allObservedEntities = tmp;
            }
            finally
            {
                _allEntitiesLock.Exit();
            }
        }

        /** sends notifications to 'OnEntityLoaded' or 'OnEntityUpdated' for anything changed since last call.
         */
        private void ProcessChangedEntities(ICollection<Entity> changedEntities)
        {
            var updated = new HashSet<Entity>();
            var created = new HashSet<Entity>();
           
            foreach (var entity in changedEntities.Where(item => !item.PendingDestruction))
            {
                bool previouslyExisted;
                var entityToAdd = UpdateEntityMaps(entity, out previouslyExisted);
                if (previouslyExisted)
                    updated.Add(entityToAdd);
                else
                    created.Add(entityToAdd);
            }

            _entityQuadTree.UpdateItems(updated.Concat(created));
            UpdateListOfEntities();

            SaveEntities(updated);
            SaveEntities(created);

            if ((created.Count > 0) && (OnEntitiesLoaded != null))
                OnEntitiesLoaded(created.ToList());

            if ((updated.Count > 0) && (OnEntitiesUpdated != null))
                OnEntitiesUpdated(updated.ToList());
        }
    }
}
