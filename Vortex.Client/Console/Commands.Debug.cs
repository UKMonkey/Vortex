using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Psy.Core;
using SlimMath;
using Vortex.Interface.EntityBase;


namespace Vortex.Client.Console
{
    internal class DebugOptions
    {
        public List<int> EntityId = new List<int>();
        public bool ObservedEntitiesOnly = true;

        public bool ShowSingle()
        {
            return EntityId.Count > 0;
        }
    }


    partial class Commands
    {
        private void ConsoleCommandDebug(string[] parameters)
        {
            var options = EstablishOptions(parameters);
            if (options == null)
            {
                _engine.ConsoleText("Invalid option specified: ");
                _engine.ConsoleText("-a :   Display all entities in the cache rather than observed only");
                _engine.ConsoleText("<EntityId> ... :  Display full info about the given entity");
                return;
            }

            if (options.ShowSingle())
            {
                ShowSingleEntity(options);
            }
            else
            {
                ShowEntityOverall(options);
            }
        }

        private DebugOptions EstablishOptions(string[] parameters)
        {
            var ret = new DebugOptions();
            for (var i=1; i<parameters.Length; ++i)
            {
                if (parameters[i].ElementAt(0) == '-')
                {
                    if (parameters[i].Length <= 1)
                        return null;

                    switch (parameters[i].ElementAt(1))
                    {
                        case 'a':
                            ret.ObservedEntitiesOnly = false;
                            break;
                        default:
                            return null;
                    }
                }
                else
                {
                    int result;
                    if (Int32.TryParse(parameters[i], out result))
                        ret.EntityId.Add(result);
                    else
                        return null;
                }
            }

            return ret;
        }

        private void ShowEntityOverall(DebugOptions options)
        {
            var entityCounts = new Dictionary<string, List<Entity>>(10);

            _engine.ConsoleText(String.Format("Logical entity count = {0}", _engine.Entities.Count()));
            var entityList = options.ObservedEntitiesOnly
                                 ? _engine.Entities
                                 : _engine.FullEntityList();

            foreach (var entity in entityList)
            {
                if (!entityCounts.ContainsKey(entity.EntityTypeName))
                {
                    entityCounts[entity.EntityTypeName] = new List<Entity>();
                }

                entityCounts[entity.EntityTypeName].Add(entity);
            }

            _engine.ConsoleText("-- Entity totals --");

            foreach (var entityCollection in entityCounts)
            {
                _engine.ConsoleText(string.Format("{0} - {1}", entityCollection.Key, entityCollection.Value.Count),
                                        new Color4(1.0f, 0.0f, 1.0f, 0.5f));

                const int maxOnOneLine = 10;
                var singleLineCount = 0;
                var stringBuilder = new StringBuilder();
                var first = true;

                foreach (var entity in entityCollection.Value)
                {
                    if (entity.EntityTypeName != entityCollection.Key)
                        continue;

                    if (!first)
                        stringBuilder.AppendFormat(", ");

                    first = false;
                    stringBuilder.AppendFormat("{0}", entity.EntityId);

                    if (singleLineCount == maxOnOneLine)
                    {
                        singleLineCount = 0;
                        _engine.ConsoleText(stringBuilder.ToString());
                        stringBuilder.Clear();
                    }
                }

                _engine.ConsoleText(stringBuilder.ToString());
                stringBuilder.Clear();
            }
        }

        private void ShowSingleEntity(DebugOptions options)
        {
            foreach (var id in options.EntityId)
            {
                var entity = options.ObservedEntitiesOnly
                                 ? _engine.Entities.FirstOrDefault(x => x.EntityId == id)
                                 : _engine.GetEntity(id);

                if (entity == null)
                {
                    _engine.ConsoleText(string.Format("Unable to locate entity {0}", id));
                    continue;
                }

                _engine.ConsoleText(string.Format("-- Entity {0} --", id));

                var properties = entity.Properties.ToList();
                var outputText = properties.Select(entityProperty => string.Format("{0} = {1}", GetEnumName(entityProperty.PropertyId), entityProperty)).ToList();

                outputText.Sort();

                foreach (var item in outputText)
                    _engine.ConsoleText(item);
            }
        }
    }
}