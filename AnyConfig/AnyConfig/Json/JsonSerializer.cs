using System;
using System.Linq;
using TypeSupport;
using TypeSupport.Extensions;

namespace AnyConfig.Json
{
    public static class JsonSerializer
    {
        public static string Serialize<T>(T value)
        {
            throw new NotImplementedException($"Json serialization is not currently supported.");
        }

        /// <summary>
        /// Deserialize json string to type <seealso cref="T"/>
        /// </summary>
        /// <typeparam name="T">The type to deserialize</typeparam>
        /// <param name="json">Json string</param>
        /// <returns></returns>
        public static T Deserialize<T>(string json)
        {
            return Deserialize<T>(json, string.Empty);
        }

        /// <summary>
        /// Deserialize json string to type <seealso cref="T"/>
        /// </summary>
        /// <typeparam name="T">The type to deserialize</typeparam>
        /// <param name="json">Json string</param>
        /// <returns></returns>
        public static object Deserialize(string json, Type type)
        {
            return Deserialize(json, string.Empty, type);
        }

        /// <summary>
        /// Deserialize json string to type <seealso cref="T"/>
        /// </summary>
        /// <typeparam name="T">The type to deserialize</typeparam>
        /// <param name="json">Json string</param>
        /// <param name="jsonNodeName">Name of type to deserialize</param>
        /// <returns></returns>
        public static T Deserialize<T>(string json, string jsonNodeName)
        {
            var parser = new JsonParser();
            var rootNode = parser.Parse(json);
            var factory = new ObjectFactory();
            var value = factory.CreateEmptyObject<T>();

            value = DeserializeNode<T>(value, rootNode);

            return value;
        }

        /// <summary>
        /// Deserialize json string to type <seealso cref="type"/>
        /// </summary>
        /// <param name="json">Json string</param>
        /// <param name="jsonNodeName">Name of json node to deserialize</param>
        /// <param name="type">The type to deserialize</param>
        /// <returns></returns>
        public static object Deserialize(string json, string jsonNodeName, Type type)
        {
            var parser = new JsonParser();
            var rootNode = parser.Parse(json);
            var factory = new ObjectFactory();
            var value = factory.CreateEmptyObject(type);

            value = DeserializeNode(type, value, rootNode, false);

            return value;
        }

        private static object DeserializeNode(ExtendedType type, object value, JsonNode node, bool skipFlatMap, LegacyConfigurationNameAttribute propertyAttribute = null)
        {
            var properties = type.Properties;

            foreach(var property in properties)
            {
                var propertyType = property.Type;
                if (propertyType.IsReferenceType && propertyType != typeof(string) && !propertyType.IsAbstract)
                {
                    var matchedNode = node.SelectNodeByName(property.Name) as JsonNode;
                    if (matchedNode != null)
                        skipFlatMap = true;
                    var objectFactory = new ObjectFactory();
                    var childValue = objectFactory.CreateEmptyObject(propertyType);
                    childValue = DeserializeNode(propertyType, childValue, matchedNode ?? node, skipFlatMap, property.GetAttribute<LegacyConfigurationNameAttribute>());
                    value.SetPropertyValue(property.Name, childValue);
                }
                else
                {
                    // find matching node name
                    var nodeName = property.Name;
                    if (!skipFlatMap)
                    {
                        var attribute = property.GetAttribute<LegacyConfigurationNameAttribute>();
                        if (attribute != null)
                            nodeName = attribute.SettingName;
                        if (!string.IsNullOrEmpty(propertyAttribute?.PrependChildrenName))
                            nodeName = $"{propertyAttribute.PrependChildrenName}{nodeName}";
                    }
                    var matchedNode = node.SelectNodeByName(nodeName) as JsonNode;
                    if (matchedNode != null)
                    {
                        switch (matchedNode.ValueType)
                        {
                            case PrimitiveTypes.Object:
                                var factory = new ObjectFactory();
                                var obj = factory.CreateEmptyObject(value.GetProperty(property.Name).PropertyType);
                                obj = DeserializeNode(value.GetProperty(property.Name).PropertyType.GetExtendedType(), obj, matchedNode, skipFlatMap);
                                value.SetPropertyValue(property.Name, obj);
                                break;
                            case PrimitiveTypes.String:
                                if (property.Type.IsEnum == true)
                                    value.SetPropertyValue(property.Name, Enum.Parse(property.Type, matchedNode.Value));
                                else
                                    value.SetPropertyValue(property.Name, matchedNode.Value);
                                break;
                            case PrimitiveTypes.Integer:
                                value.SetPropertyValue(property.Name, int.Parse(matchedNode.Value));
                                break;
                            case PrimitiveTypes.Boolean:
                                value.SetPropertyValue(property.Name, bool.Parse(matchedNode.Value));
                                break;
                            case PrimitiveTypes.Array:
                                value.SetPropertyValue(property.Name, matchedNode);
                                break;
                            case PrimitiveTypes.Number:
                                value.SetPropertyValue(property.Name, double.Parse(matchedNode.Value));
                                break;
                            case PrimitiveTypes.Null:
                            default:
                                // nothing to do
                                break;
                        }
                    }
                }
            }

            return value;
        }

        private static T DeserializeNode<T>(T value, JsonNode node)
        {
            return (T)DeserializeNode(typeof(T).GetExtendedType(), value, node, false);
        }
    }
}
