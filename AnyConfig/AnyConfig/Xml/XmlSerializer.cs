using System;
using System.Linq;
using TypeSupport;
using TypeSupport.Extensions;

namespace AnyConfig.Xml
{
    public static class XmlSerializer
    {
        public static string Serialize<T>(T value)
        {
            throw new NotImplementedException($"Xml serialization is not currently supported.");
        }

        /// <summary>
        /// Deserialize Xml to type <seealso cref="T"/>
        /// </summary>
        /// <typeparam name="T">The type to deserialize to</typeparam>
        /// <param name="xml">Xml string</param>
        /// <returns></returns>
        public static T Deserialize<T>(string xml)
        {
            return Deserialize<T>(xml, string.Empty);
        }

        /// <summary>
        /// Deserialize Xml to type <seealso cref="type"/>
        /// </summary>
        /// <param name="xml">Xml string</param>
        /// <param name="type">The type to deserialize to</param>
        /// <returns></returns>
        public static object Deserialize(string xml, Type type)
        {
            return Deserialize(xml, string.Empty, type);
        }

        /// <summary>
        /// Deserialize Xml to type <seealso cref="type"/>
        /// </summary>
        /// <param name="xml">Xml string</param>
        /// <param name="rootXmlElementName">The name of the root element</param>
        /// <param name="type">The type to deserialize to</param>
        /// <returns></returns>
        public static object Deserialize(string xml, string rootXmlElementName, Type type)
        {
            var parser = new XmlParser();
            var rootNode = parser.Parse(xml);
            var factory = new ObjectFactory();
            var value = factory.CreateEmptyObject(type);

            // process the root node
            if (string.IsNullOrEmpty(rootXmlElementName) || rootNode.Name.Equals(rootXmlElementName, StringComparison.InvariantCultureIgnoreCase))
            {
                value = DeserializeNode(type, value, rootNode);
            }

            return value;
        }

        /// <summary>
        /// Deserialize Xml to type <seealso cref="T"/>
        /// </summary>
        /// <typeparam name="T">The type to deserialize to</typeparam>
        /// <param name="xml">Xml string</param>
        /// <param name="rootXmlElementName">The name of the root element</param>
        /// <returns></returns>
        public static T Deserialize<T>(string xml, string rootXmlElementName)
        {
            var parser = new XmlParser();
            var rootNode = parser.Parse(xml);
            var factory = new ObjectFactory();
            var value = factory.CreateEmptyObject<T>();

            // process the root node
            if (string.IsNullOrEmpty(rootXmlElementName) || rootNode.Name.Equals(rootXmlElementName, StringComparison.InvariantCultureIgnoreCase))
            {
                value = DeserializeNode<T>(value, rootNode);
            }

            return value;
        }

        private static object DeserializeNodeArray(Type type, object value, XmlNode node)
        {
            var extendedType = type.GetExtendedType();
            var genericArgumentType = extendedType.GenericArgumentTypes.First();
            var factory = new ObjectFactory();
            var properties = extendedType.Properties;
            var addMethod = extendedType.Methods.FirstOrDefault(x => x.Name == "Add" && x.Parameters.Any(y => y.ParameterType == genericArgumentType));
            foreach (var childNode in node.ChildNodes)
            {
                // add to array
                var arrayItem = factory.CreateEmptyObject(genericArgumentType);
                var nodeArrayItem = DeserializeNode(genericArgumentType, arrayItem, childNode);
                addMethod.MethodInfo.Invoke(value, new object[] { nodeArrayItem });
            }
            return value;
        }

        private static object DeserializeNode(Type type, object value, XmlNode node)
        {
            var extendedType = type.GetExtendedType();
            var properties = extendedType.Properties;

            foreach (var childNode in node.ChildNodes)
            {
                var property = properties.FirstOrDefault(x => x.Name == childNode.Name);
                if (property == null)
                    continue;
                switch (property.Type.Name)
                {
                    case "Boolean":
                        value.SetPropertyValue(childNode.Name, bool.Parse(childNode.InnerContent));
                        break;
                    case "Byte":
                        value.SetPropertyValue(childNode.Name, byte.Parse(childNode.InnerContent));
                        break;
                    case "SByte":
                        value.SetPropertyValue(childNode.Name, sbyte.Parse(childNode.InnerContent));
                        break;
                    case "UInt16":
                        value.SetPropertyValue(childNode.Name, ushort.Parse(childNode.InnerContent));
                        break;
                    case "Int16":
                        value.SetPropertyValue(childNode.Name, short.Parse(childNode.InnerContent));
                        break;
                    case "UInt32":
                        value.SetPropertyValue(childNode.Name, uint.Parse(childNode.InnerContent));
                        break;
                    case "Int32":
                        value.SetPropertyValue(childNode.Name, int.Parse(childNode.InnerContent));
                        break;
                    case "UInt64":
                        value.SetPropertyValue(childNode.Name, ulong.Parse(childNode.InnerContent));
                        break;
                    case "Int64":
                        value.SetPropertyValue(childNode.Name, long.Parse(childNode.InnerContent));
                        break;
                    case "Single":
                        value.SetPropertyValue(childNode.Name, float.Parse(childNode.InnerContent));
                        break;
                    case "Double":
                        value.SetPropertyValue(childNode.Name, double.Parse(childNode.InnerContent));
                        break;
                    case "Decimal":
                        value.SetPropertyValue(childNode.Name, decimal.Parse(childNode.InnerContent));
                        break;
                    case "String":
                        value.SetPropertyValue(childNode.Name, childNode.InnerContent);
                        break;
                    default:
                        // custom type
                        if (property.Type.IsEnum)
                        {
                            var enumValue = Enum.Parse(property.Type, childNode.InnerContent);
                            value.SetPropertyValue(childNode.Name, enumValue);
                            break;
                        }
                        var factory = new ObjectFactory();
                        var obj = factory.CreateEmptyObject(property.Type);
                        var propertyExtendedType = property.Type.GetExtendedType();
                        if (propertyExtendedType.IsEnumerable)
                        {
                            obj = DeserializeNodeArray(property.Type, obj, childNode);
                        }
                        else
                        {
                            obj = DeserializeNode(property.Type, obj, childNode);
                        }
                        value.SetPropertyValue(childNode.Name, obj);
                        break;
                }
            }

            return value;
        }

        private static T DeserializeNode<T>(T value, XmlNode node)
        {
            return (T)DeserializeNode(typeof(T), value, node);
        }
    }
}
