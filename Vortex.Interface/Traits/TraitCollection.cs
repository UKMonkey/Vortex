using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vortex.Interface.Traits
{
    public class TraitCollection<TraitType> where TraitType: Trait
    {
        public event TraitCallback OnTraitChanged;

        private readonly Dictionary<int, TraitType> _nonDefaultProperties;
        private readonly Dictionary<int, TraitType> _defaultProperties;

        public IEnumerable<TraitType> Properties { get { return _nonDefaultProperties.Values.Concat(_defaultProperties.Values); } }
        public int PropertyCount { get { return _nonDefaultProperties.Count + _defaultProperties.Count; } }

        public IEnumerable<TraitType> NonDefaultProperties { get { return _nonDefaultProperties.Values; } }
        public int NonDefaultPropertyCount { get { return _nonDefaultProperties.Count; } }


        protected TraitCollection()
        {
            _nonDefaultProperties = new Dictionary<int, TraitType>();
            _defaultProperties = new Dictionary<int, TraitType>();
        }


        public TraitType GetProperty(int property)
        {
            if (_nonDefaultProperties.ContainsKey(property))
                return _nonDefaultProperties[property];
            if (_defaultProperties.ContainsKey(property))
                return _defaultProperties[property];
            throw new Exception("Entity property accessed when not available");
        }

        public void SetDefaultProperty(TraitType property)
        {
            _defaultProperties[property.PropertyId] = property;
            property.OnTraitChanged += DefaultPropertyChanged;
            UpdateCachedProperties(property);
        }

        public void SetProperties(IEnumerable<TraitType> properties)
        {
            foreach (var prop in properties)
                SetProperty(prop);
        }

        public void SetProperty(TraitType property)
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

        public void ClearProperty(int property)
        {
            _nonDefaultProperties.Remove(property);
            _defaultProperties.Remove(property);
        }

        public bool HasProperty(int propertyKey)
        {
            return _nonDefaultProperties.ContainsKey(propertyKey) || _defaultProperties.ContainsKey(propertyKey);
        }

        protected void UpdateCachedProperties(Trait property)
        {
        }

        public bool IsDirty()
        {
            foreach (var property in _nonDefaultProperties)
                if (property.Value.IsDirty)
                    return true;
            return false;
        }

#region Private
        private void PropertyChanged(Trait property)
        {
            UpdateCachedProperties(property);

            if (OnTraitChanged != null)
                OnTraitChanged(property);
        }

        private void DefaultPropertyChanged(Trait changed)
        {
            _defaultProperties.Remove(changed.PropertyId);
            _nonDefaultProperties[changed.PropertyId] = (TraitType)changed;

            changed.OnTraitChanged -= DefaultPropertyChanged;
            changed.OnTraitChanged += PropertyChanged;

            PropertyChanged(changed);
        }
#endregion
    }
}
