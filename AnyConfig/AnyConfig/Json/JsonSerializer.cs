﻿using System;
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

            foreach (var node in rootNode.ChildNodes)
            {
                if (string.IsNullOrEmpty(jsonNodeName) || node.Name.Equals(jsonNodeName))
                {
                    value = DeserializeNode<T>(value, node);
                    break;
                }
            }

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

            foreach (var node in rootNode.ChildNodes)
            {
                if (string.IsNullOrEmpty(jsonNodeName) || node.Name.Equals(jsonNodeName))
                {
                    value = DeserializeNode(type, value, node);
                    break;
                }
            }

            return value;
        }

        private static object DeserializeNode(Type type, object value, JsonNode node)
        {
            var extendedType = type.GetExtendedType();
            var properties = extendedType.Properties;

            foreach (var childNode in node.ChildNodes)
            {
                var property = properties.FirstOrDefault(x => x.Name == childNode.Name);
                if (property == null)
                    continue;
                switch (childNode.ValueType)
                {
                    case PrimitiveTypes.Object:
                        var factory = new ObjectFactory();
                        var obj = factory.CreateEmptyObject(value.GetProperty(childNode.Name).PropertyType);
                        obj = DeserializeNode(value.GetProperty(childNode.Name).PropertyType, obj, childNode);
                        value.SetPropertyValue(childNode.Name, obj);
                        break;
                    case PrimitiveTypes.String:
                        if (property.Type.IsEnum == true)
                            value.SetPropertyValue(childNode.Name, Enum.Parse(property.Type, childNode.Value));
                        else
                            value.SetPropertyValue(childNode.Name, childNode.Value);
                        break;
                    case PrimitiveTypes.Integer:
                        value.SetPropertyValue(childNode.Name, int.Parse(childNode.Value));
                        break;
                    case PrimitiveTypes.Boolean:
                        value.SetPropertyValue(childNode.Name, bool.Parse(childNode.Value));
                        break;
                    case PrimitiveTypes.Array:
                        value.SetPropertyValue(childNode.Name, childNode);
                        break;
                    case PrimitiveTypes.Number:
                        value.SetPropertyValue(childNode.Name, double.Parse(childNode.Value));
                        break;
                    case PrimitiveTypes.Null:
                        // nothing to do
                        break;
                }
            }

            return value;
        }

        private static T DeserializeNode<T>(T value, JsonNode node)
        {
            return (T)DeserializeNode(typeof(T), value, node);
        }
    }
}
