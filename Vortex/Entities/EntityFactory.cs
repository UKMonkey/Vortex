using System;
using System.Collections.Generic;
using Psy.Graphics.Models;
using Vortex.Interface;
using Vortex.Interface.EntityBase;
using Vortex.Interface.EntityBase.Behaviours;
using Vortex.Interface.EntityBase.Damage;
using Vortex.Interface.EntityBase.Properties;

namespace Vortex.Entities
{
    public class EntityFactory : IEntityFactory
    {
        private readonly CompiledModelCache _compiledModelCache;
        private readonly Dictionary<int, string> _nameLookup;
        private readonly Dictionary<string, short> _entityTypeIdLookup;
        private readonly Dictionary<short, Dictionary<short, List<IEntityBehaviour>>> _behaviours;
        private readonly Dictionary<short, List<EntityProperty>> _defaultProperties;
        private readonly Dictionary<short, IEntityDamageHandler> _defaultDamageHandlers;
        private readonly Dictionary<short, Dictionary<DamageTypeEnum, IEntityDamageHandler>> _damageHandlers;

        private int _nextEntityId;
        private short _lastEntityTypeAdded;

        public EntityFactory(CompiledModelCache compiledModelCache)
        {
            _compiledModelCache = compiledModelCache;
            _nameLookup = new Dictionary<int, string>();
            _entityTypeIdLookup = new Dictionary<string, short>();
            _behaviours = new Dictionary<short, Dictionary<short, List<IEntityBehaviour>>>();
            _defaultProperties = new Dictionary<short, List<EntityProperty>>();
            _damageHandlers = new Dictionary<short, Dictionary<DamageTypeEnum, IEntityDamageHandler>>();
            _defaultDamageHandlers = new Dictionary<short, IEntityDamageHandler>();

            _nextEntityId = 0;
        }

        public IEntityFactory Add(short entityTypeId, string name)
        {
            _lastEntityTypeAdded = entityTypeId;

            _behaviours.Add(_lastEntityTypeAdded, new Dictionary<short, List<IEntityBehaviour>>());
            _defaultProperties.Add(_lastEntityTypeAdded, new List<EntityProperty>());
            _damageHandlers[_lastEntityTypeAdded] = new Dictionary<DamageTypeEnum, IEntityDamageHandler>();
            _defaultDamageHandlers[_lastEntityTypeAdded] = null;

            _nameLookup[entityTypeId] = name;
            _entityTypeIdLookup[name] = entityTypeId;

            return this;
        }

        public IEntityFactory RegisterBehaviour(short entityTypeId, short behaviourId, IEntityBehaviour behaviour)
        {
            var data = _behaviours[entityTypeId];
            if (!data.ContainsKey(behaviourId))
                data.Add(behaviourId, new List<IEntityBehaviour>());

            data[behaviourId].Add(behaviour);
            return this;
        }

        public IEntityFactory RegisterDefaultDamageHandler(short entityTypeId, IEntityDamageHandler handler)
        {
            _defaultDamageHandlers[entityTypeId] = handler;
            return this;
        }

        public IEntityFactory RegisterDamageHandler(short entityTypeId, IEntityDamageHandler handler, DamageTypeEnum type)
        {
            _damageHandlers[entityTypeId][type] = handler;
            return this;
        }

        public IEntityFactory RegisterDefaultProperty(EntityProperty property)
        {
            var data = _defaultProperties[_lastEntityTypeAdded];
            data.Add(property);
            return this;
        }

        public int GetRegisteredCount()
        {
            return _nameLookup.Count;
        }

        public Entity Get(short entityTypeId)
        {
            if (!_nameLookup.ContainsKey(entityTypeId))
                throw new Exception(string.Format("Unable to generate entity of type {0}", entityTypeId));

            var entityProperties = _defaultProperties[entityTypeId];

            var entity = new Entity(_compiledModelCache, entityTypeId, _nameLookup[entityTypeId]);

            foreach (var item in _behaviours[entityTypeId])
                entity.SetBehaviour(item.Key, item.Value);
            
            foreach (var item in entityProperties)
                entity.SetDefaultProperty(item.Clone());

            foreach (var item in entity.Properties)
                item.ClearDirtyFlag();

            foreach (var item in _damageHandlers[entityTypeId])
                entity.SetDamageHandler(item.Key, item.Value);

            entity.SetDefaultDamageHandler(_defaultDamageHandlers[entityTypeId]);
            entity.EntityId = ++_nextEntityId;

            return entity;
        }
    }
}
