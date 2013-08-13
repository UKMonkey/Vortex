using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Psy.Core;
using Psy.Core.Collision;
using Psy.Core.EpicModel;
using Psy.Graphics.Models;
using SlimMath;
using Vortex.Interface.EntityBase.Behaviours;
using Vortex.Interface.EntityBase.Damage;
using Vortex.Interface.EntityBase.Properties;
using Vortex.Interface.Traits;

namespace Vortex.Interface.EntityBase
{
    public delegate void EntityCallback(Entity entity);
    public delegate void EntityPropertyCallback(Entity entity, Trait prop);
    public delegate IEnumerable<Entity> EntityCollection();
    public delegate bool EntityTester(Entity eyes, Entity item, IList<Entity> otherEntities);
    public delegate void EntityHandler(IEnumerable<Entity> entities);

    [DebuggerDisplay("Id:{EntityId} TypeId:{EntityTypeId} TypeName:{EntityTypeName}")]
    public class Entity
    {
#region Properties
        private readonly Dictionary<int, EntityProperty> _nonDefaultProperties;
        private readonly Dictionary<int, EntityProperty> _defaultProperties;
         
        public IEnumerable<EntityProperty> Properties { get { return _nonDefaultProperties.Values.Concat(_defaultProperties.Values); } }
        public int PropertyCount { get { return _nonDefaultProperties.Count + _defaultProperties.Count; } }

        public IEnumerable<EntityProperty> NonDefaultProperties { get { return _nonDefaultProperties.Values; } }
        public int NonDefaultPropertyCount { get { return _nonDefaultProperties.Count; } }

        public EntityProperty GetProperty(int property)
        {
            if (_nonDefaultProperties.ContainsKey(property))
                return _nonDefaultProperties[property];
            if (_defaultProperties.ContainsKey(property))
                return _defaultProperties[property];
            throw new Exception("Entity property accessed when not available");
        }

        public void SetDefaultProperty(EntityProperty property)
        {
            _defaultProperties[property.PropertyId] = property;
            property.OnTraitChanged += DefaultPropertyChanged;
            UpdateCachedProperties(property);
        }

        public void SetProperties(IEnumerable<EntityProperty> properties)
        {
            foreach (var prop in properties)
                SetProperty(prop);
        }

        public void SetProperty(EntityProperty property)
        {
            var propertyKey = property.PropertyId;

            if (_nonDefaultProperties.ContainsKey(propertyKey))
            {
                var entityProperty = _nonDefaultProperties[propertyKey];
                entityProperty.Value = property.Value;
            }
            else if (_defaultProperties.ContainsKey(propertyKey))
            {
                var baseProperty = _defaultProperties[propertyKey];
                _defaultProperties.Remove(propertyKey);

                _nonDefaultProperties.Add(propertyKey, baseProperty);
                baseProperty.Value = property.Value;
            }
            else
            {
                _nonDefaultProperties[propertyKey] = property;
                property.OnTraitChanged += PropertyChanged;
                PropertyChanged(property);
            }                
        }

        private void DefaultPropertyChanged(Trait changed)
        {
            _defaultProperties.Remove(changed.PropertyId);
            _nonDefaultProperties[changed.PropertyId] = (EntityProperty)changed;
            
            changed.OnTraitChanged -= DefaultPropertyChanged;
            changed.OnTraitChanged += PropertyChanged;

            PropertyChanged(changed);
        }

        public void ClearProperty(int property)
        {
            _nonDefaultProperties.Remove(property);
            _defaultProperties.Remove(property);
        }

        public bool HasProperty(int propertyKey)
        {
            return _nonDefaultProperties.ContainsKey(propertyKey) || _defaultProperties.ContainsKey(propertyKey);
        }

        private void UpdateCachedProperties(Trait property)
        {
            switch (property.PropertyId)
            {
                case (short)EntityPropertyEnum.Position:
                    _position = property.VectorValue;
                    break;
                case (short)EntityPropertyEnum.Rotation:
                    _rotation = property.FloatValue;
                    break;
                case (short)EntityPropertyEnum.Nameplate:
                    _nameplate = property.StringValue;
                    break;
                case (short)EntityPropertyEnum.NameplateColour:
                    _nameplateColour = property.ColourValue;
                    break;
                case (short)EntityPropertyEnum.Model:
                    SetModel(property.StringValue, propagate:false);
                    break;
            }
        }

        private void PropertyChanged(Trait property)
        {
            UpdateCachedProperties(property);

            if (OnPropertyChanged != null)
                OnPropertyChanged(this, property);
        }

#endregion

        /** These are properties that should still be kept in sync between client & server, but 
         *  it is important that since they are called by the renderer we don't do a map lookup to get the value
         */
#region CachedData
        private Vector3 _position;
        public Vector3 GetPosition()
        {
            return _position;
        }

        public Vector3 GetEyePosition()
        {
            return _position + new Vector3(0, 0, Model.ModelInstance.TopVertexZ * 3f / 4f);
        }

        public void SetPosition(Vector3 value)
        {
            GetProperty((int)EntityPropertyEnum.Position).VectorValue = value;
        }

        private float _rotation;
        public float GetRotation()
        {
            return _rotation;
        }

        public void SetRotation(float value)
        {
            GetProperty((int)EntityPropertyEnum.Rotation).FloatValue = EntityPropertyExtensions.LimitAngle(value);
        }


        private string _nameplate;
        public string GetNameplate()
        {
            return _nameplate;
        }

        public void SetNameplate(string value)
        {
            GetProperty((int)EntityPropertyEnum.Nameplate).StringValue = value;
        }

        private Color4 _nameplateColour;
        public Color4 GetNameplateColour()
        {
            return _nameplateColour;
        }

        public void SetNameplateColour(Color4 value)
        {
            GetProperty((int)EntityPropertyEnum.NameplateColour).ColourValue = value;
        }
#endregion

#region Behaviours
        private readonly Dictionary<short, IEnumerable<IEntityBehaviour>> _behaviours;
        public void SetBehaviour(short behaviourId, IEnumerable<IEntityBehaviour> behaviour)
        {
            if (_behaviours.ContainsKey(behaviourId))
                _behaviours.Remove(behaviourId);

            if (behaviour != null)
                _behaviours.Add(behaviourId, behaviour);
        }

        public void PerformBehaviour(short behaviourId, Entity instagator)
        {
            if (!_behaviours.ContainsKey(behaviourId))
                return;

            foreach (var behaviour in _behaviours[behaviourId])
                behaviour.PerformBehaviour(this, instagator);
        }
#endregion

        public float Radius { get { return Model.ModelInstance.Radius; } }

        public EntityModel Model { get; set; }

        private readonly CompiledModelCache _compiledModelCache;
        public readonly short EntityTypeId;
        public readonly string EntityTypeName;
        public bool Registered { get; set; }

        public bool PendingDestruction { get; set; }
        public int Parent { get; set; }

        public int EntityId { get; set; }

        public Mesh Mesh
        {
            get
            {
                var mesh = Model.ModelInstance.GetCollisionMesh();

                if (mesh != null)
                {
                    mesh.Rotation = GetRotation();
                    mesh.Translation = GetPosition();

                    // todo: improve this, maybe mesh.Owner?
                    mesh.Id = EntityId;
                }
                return mesh;
            }
        }

        public event EntityPropertyCallback OnPropertyChanged;
        public event EntityCallback OnDeath;

        public Entity(CompiledModelCache compiledModelCache, short entityTypeId, string entityTypeName)
        {
            Registered = false;
            _compiledModelCache = compiledModelCache;
            Model = new EntityModel(compiledModelCache);
            EntityTypeId = entityTypeId;
            EntityTypeName = entityTypeName;
            EntityId = 0;

            _nonDefaultProperties = new Dictionary<int, EntityProperty>();
            _defaultProperties = new Dictionary<int, EntityProperty>();
            _behaviours = new Dictionary<short, IEnumerable<IEntityBehaviour>>();
            _damageHandlers = new Dictionary<DamageTypeEnum, IEntityDamageHandler>();

            ApplyDefaultProperties();
        }

        public void SetModel(string model, bool propagate=true)
        {
            var property = GetProperty((int)EntityPropertyEnum.Model);
            var previousModel = property.StringValue;

            if (previousModel != model || Model.ModelInstance == null)
            {
                if (propagate)
                {
                    property.StringValue = model;
                }

                if (previousModel != model || Model.ModelInstance == null)
                {
                    Model.SetModel(model);
                }
            }
        }

        private void ApplyDefaultProperties()
        {
            SetDefaultProperty(new EntityProperty((int)EntityPropertyEnum.Solid, false));
            SetDefaultProperty(new EntityProperty((int)EntityPropertyEnum.Static, false));
            SetDefaultProperty(new EntityProperty((int)EntityPropertyEnum.MaxHealth, 1));
            SetDefaultProperty(new EntityProperty((int)EntityPropertyEnum.Health, 1));
            SetDefaultProperty(new EntityProperty((int)EntityPropertyEnum.MovementVector, new Vector3(0, 0, 0)));
            SetDefaultProperty(new EntityProperty((int)EntityPropertyEnum.Position, new Vector3(0, 0, 0)));
            SetDefaultProperty(new EntityProperty((int)EntityPropertyEnum.Rotation, 0f));
            SetDefaultProperty(new EntityProperty((int)EntityPropertyEnum.Nameplate, ""));
            SetDefaultProperty(new EntityProperty((int)EntityPropertyEnum.NameplateColour, Colours.White));
            SetDefaultProperty(new EntityProperty((int)EntityPropertyEnum.TypeName, EntityTypeName));
        }

        public bool IsDirty()
        {
            foreach (var property in _nonDefaultProperties)
                if (property.Value.IsDirty)
                    return true;
            return false;
        }

#region Destruction
        public void Destroy()
        {
            if (PendingDestruction)
                return;

            PendingDestruction = true;

            InvokeDeadEvents();
        }

        private void InvokeDeadEvents()
        {
            if (OnDeath != null)
                OnDeath(this);
        }
#endregion

#region Movement
        public void LookAt(Vector3 target)
        {
            var x = target.X - this.GetPosition().X;
            var y = target.Y - this.GetPosition().Y;
            var angle = Math.Atan2(y, x);

            this.SetRotation((float)angle);
        }

        // TODO - use the entity collision meshes
        public bool CollidesWith(Entity otherEntity)
        {
            var distance = this.GetPosition().Distance(otherEntity.GetPosition());
            var maxDistance = Radius + otherEntity.Radius;

            if (distance > maxDistance)
                return false;
            return true;
            //return new MeshCollisionTester(otherEntity.Mesh).MeshesCollide(Mesh);
        }
#endregion

#region Damage
        private readonly Dictionary<DamageTypeEnum, IEntityDamageHandler> _damageHandlers;
        private IEntityDamageHandler _defaultDamageHandler;

        public void TakeDamage(float amount, DamageTypeEnum damageType, Entity damageDealer)
        {
            var damage = 0;

            if (_damageHandlers.ContainsKey(damageType))
                damage = _damageHandlers[damageType].EstablishDamage(this, amount, damageType, damageDealer);
            else if (_defaultDamageHandler != null)
                damage = _defaultDamageHandler.EstablishDamage(this, amount, damageType, damageDealer);

            if (damage <= 0)
                return;

            var health = this.GetHealth();
            this.SetHealth(health - damage);
        }

        public void SetDefaultDamageHandler(IEntityDamageHandler handler)
        {
            _defaultDamageHandler = handler;
        }

        public void SetDamageHandler(DamageTypeEnum type, IEntityDamageHandler handler)
        {
            if (handler == null)
                _damageHandlers.Remove(type);
            else
                _damageHandlers[type] = handler;
        }
#endregion

        public void UpdateAnimation()
        {
            if (this.GetMovementVector().Length > 0.01f)
            {
                Model.ModelInstance.RunAnimation(AnimationType.Walking, true);
            }
            else
            {
                Model.ModelInstance.StopAnimation(AnimationType.Walking);
            }

            if (Model != null)
            {
                Model.ModelInstance.Update(1/24.0f);
            }
        }
    }
}
