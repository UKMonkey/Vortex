using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SlimMath;

namespace Vortex.Interface.World.Triggers
{
    public class TriggerFactory : ITriggerFactory
    {
        private readonly Dictionary<string, ConstructorInfo> _nameToTrigger;
        private readonly IEngine _engine;

        public TriggerFactory(IEngine engine)
        {
            _nameToTrigger = new Dictionary<string, ConstructorInfo>();
            _engine = engine;
            var triggers = DiscoverTriggers();
            AssignTriggers(triggers);
        }

        public ITrigger GetTrigger(string type, TriggerKey key, Vector3 position)
        {
            return GetTrigger(type, key, position, new List<KeyValuePair<string, string>>());
        }

        public ITrigger GetTrigger(string type, TriggerKey key, Vector3 position, IEnumerable<KeyValuePair<string, string>> properties)
        {
            var constructor = _nameToTrigger[type];
            var instance = (ITrigger)constructor.Invoke(new object[] { _engine });

            instance.SetProperties(key, position, properties);
            return instance;
        }

        private void AssignTriggers(IEnumerable<Type> triggers)
        {
            foreach (var item in triggers)
            {
                var constructor = item.GetConstructor(new[] {typeof(IEngine)});
                if (constructor == null)
                    continue;
                var instance = (ITrigger)constructor.Invoke(new object[] {_engine});
                _nameToTrigger.Add(instance.Name, constructor);
            }
        }

        private IEnumerable<Type> DiscoverTriggersInAssembly(Assembly assembly)
        {
            var availableTypes = assembly.GetExportedTypes();

            return availableTypes.Where(item => typeof(ITrigger).IsAssignableFrom(item));
        }

        private IEnumerable<Type> DiscoverTriggers()
        {
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            var availableMessages = new List<Type>();

            foreach (var assembly in loadedAssemblies)
            {
                availableMessages.AddRange(DiscoverTriggersInAssembly(assembly));
            }

            return availableMessages;
        }
    }
}
