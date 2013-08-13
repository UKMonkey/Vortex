using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Psy.Core;
using Psy.Core.FileSystem;
using Vortex.Interface.EntityBase.Properties;

namespace Vortex.Interface
{
    public class EntityType
    {
        private readonly List<KeyValuePair<int, EntityProperty>> _initialProperties;
        public IEnumerable<KeyValuePair<int, EntityProperty>> InitialProperties { get { return _initialProperties; } }


        private EntityType(string modelFileName,
            IEnumerable<KeyValuePair<int, EntityProperty>> defaultProperties)
        {
            _initialProperties = defaultProperties.ToList();
        }


        /*****************************************/
        /** Statics **/


        public static EntityType LoadFromFile(string filename, Type gamePropertyEnum)
        {
            var document = new XmlDocument();
            document.Load(Lookup.GetAssetPath(filename));

            var rootNode = document.DocumentElement;
            var modelName = GetModelName(rootNode);
            var defaultProperties = GetDefaultProperties(rootNode, gamePropertyEnum);

            if (modelName == "")
                throw new AssetLoadException("imagename cannot be blank");

            return new EntityType(modelName, defaultProperties);
        }

        private static string GetModelName(XmlNode rootNode)
        {
            var values = (from XmlNode item
                                 in rootNode.ChildNodes
                          where item.Name.Equals("modelname")
                          select item.InnerText)
                         .ToList();

            if (values.Count != 1)
                return "";

            return values[0];
        }

        private static IEnumerable<KeyValuePair<int, EntityProperty>> GetDefaultProperties(XmlNode rootNode, Type extraEnumType)
        {
            var enumProperties = GetEnumBreakdown(new[] { extraEnumType, typeof(EntityPropertyEnum) });
            var props = (from XmlNode item
                                 in rootNode.ChildNodes
                         where item.Name.Equals("properties")
                         select item)
                         .ToList();

            if (props.Count > 1)
                throw new Exception("Can only have 1 properties section");

            var ret = new Dictionary<int, EntityProperty>();
            if (props.Count == 0)
                return ret;

            foreach (XmlNode item in props[0].ChildNodes)
            {
                var extra = ReadProperty(item, enumProperties);
                if (ret.ContainsKey(extra.Key))
                    throw new Exception("One property was specified twice for the same entity");
                ret.Add(extra.Key, extra.Value);
            }

            return ret;
        }

        private static Dictionary<string, short> GetEnumBreakdown(Type[] types)
        {
            var ret = new Dictionary<string, short>();

            foreach (var type in types)
            {
                if (type == null)
                    continue;

                var values = Enum.GetValues(type);
                var names = Enum.GetNames(type);

                for (var i = 0; i < values.Length; ++i)
                {
                    ret.Add(names[i], (short)(values.GetValue(i)));
                }
            }
            return ret;
        }

        private static KeyValuePair<short, EntityProperty> ReadProperty(XmlNode node, Dictionary<string, short> propertyValues)
        {
            var prop = (from XmlNode item
                                 in node.ChildNodes
                        where item.Name.Equals("name")
                        select item.InnerText)
                            .ToList();
            var value = (from XmlNode item
                                 in node.ChildNodes
                         where item.Name.Equals("value")
                         select item.InnerText.ToCharArray())
                            .ToList();

            if (prop.Count != 1 && value.Count != 1)
                throw new Exception("There's something very wrong with your property here");

            var property = propertyValues[prop[0]];
            var val = Convert.FromBase64CharArray(value[0], 0, value[0].Length);

            return new KeyValuePair<short, EntityProperty>(property, new EntityProperty(property, val));
        }
    }
}
