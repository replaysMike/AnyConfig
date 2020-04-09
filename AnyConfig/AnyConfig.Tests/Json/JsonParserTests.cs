using AnyConfig.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AnyConfig.Tests.Json
{
    [TestFixture]
    public class JsonParserTests
    {
        Assembly _assembly;
        JsonParser _parser;

        [Test]
        public void ShouldParseRootObject()
        {
            var responseJson = LoadTestData("RootObjectResponse.json");
            var node = _parser.Parse(responseJson);

            Assert.That(node.NodeType, Is.EqualTo(JsonNodeType.Object));
            Assert.That(node.ParentNode, Is.Null); // ensure we are at the root
            Assert.That(node.ChildNodes.Count, Is.EqualTo(1));
        }

        [Test]
        public void ShouldParseRootArray()
        {
            var responseJson = LoadTestData("RootArrayResponse.json");
            var node = _parser.Parse(responseJson);

            Assert.That(node.NodeType, Is.EqualTo(JsonNodeType.Array));
            Assert.That(node.ParentNode, Is.Null); // ensure we are at the root
            Assert.That(node.ChildNodes.Count, Is.EqualTo(5));
        }

        [Test]
        public void ShouldParseArray()
        {
            var responseJson = LoadTestData("ArrayResponse.json");
            var node = _parser.Parse(responseJson);

            Assert.That(node.NodeType, Is.EqualTo(JsonNodeType.Object));
            Assert.That(node.ParentNode, Is.Null); // ensure we are at the root
            Assert.That(node.ChildNodes.Count, Is.EqualTo(1));
            Assert.That(node.ChildNodes.Where(x => x.Name.Equals("myArray")).First().As<JsonNode>().ValueType, Is.EqualTo(PrimitiveTypes.Array));
            Assert.That(node.ChildNodes.Where(x => x.Name.Equals("myArray")).First().ChildNodes.Count, Is.EqualTo(5));
        }

        /// <summary>
        /// Test basic structure of json
        /// </summary>
        [Test]
        public void ShouldParseSimpleJson()
        {
            var responseJson = LoadTestData("SimpleResponse.json");
            var node = _parser.Parse(responseJson);

            Assert.That(node.NodeType, Is.EqualTo(JsonNodeType.Object));
            Assert.That(node.ParentNode, Is.Null); // ensure we are at the root
            Assert.That(node.ChildNodes.Count, Is.EqualTo(4));
            Assert.That(node.ChildNodes.Where(x => x.Name.Equals("intValue")).First().As<JsonNode>().Value, Is.EqualTo("1"));
            Assert.That(node.ChildNodes.Where(x => x.Name.Equals("stringValue")).First().As<JsonNode>().Value, Is.EqualTo("string"));
            Assert.That(node.ChildNodes.Where(x => x.Name.Equals("boolValue")).First().As<JsonNode>().Value, Is.EqualTo("true"));
            Assert.That(node.ChildNodes.Where(x => x.Name.Equals("numberValue")).First().As<JsonNode>().Value, Is.EqualTo("3.1415"));
        }

        /// <summary>
        /// Tests all features of a json structure
        /// </summary>
        [Test]
        public void ShouldParseComplexJson()
        {
            var responseJson = LoadTestData("ComplexResponse.json");
            var node = _parser.Parse(responseJson);

            // validate root note
            Assert.That(node.NodeType, Is.EqualTo(JsonNodeType.Object));
            Assert.That(node.ParentNode, Is.Null); // ensure we are at the root

            // validate root children
            Assert.That(node.ChildNodes.Count, Is.EqualTo(8));
            Assert.That(node.ChildNodes.Where(x => x.Name.Equals("intValue")).First().As<JsonNode>().Value, Is.EqualTo("1"));
            Assert.That(node.ChildNodes.Where(x => x.Name.Equals("stringValue")).First().As<JsonNode>().Value, Is.EqualTo("string"));
            Assert.That(node.ChildNodes.Where(x => x.Name.Equals("boolValue")).First().As<JsonNode>().Value, Is.EqualTo("true"));
            Assert.That(node.ChildNodes.Where(x => x.Name.Equals("numberValue")).First().As<JsonNode>().Value, Is.EqualTo("3.1415"));

            // validate simple object
            var sutNode = node.ChildNodes.Where(x => x.Name.Equals("myObject")).First().As<JsonNode>();
            Assert.That(sutNode.ValueType, Is.EqualTo(PrimitiveTypes.Object));
            Assert.That(sutNode.ChildNodes.Count, Is.EqualTo(2));
            Assert.That(sutNode.ChildNodes.Where(y => y.Name.Equals("nameValue")).First().As<JsonNode>().Value, Is.EqualTo("objectname"));

            // validate nested object
            sutNode = node.ChildNodes.Where(x => x.Name.Equals("nestedObject")).First().As<JsonNode>();
            Assert.That(sutNode.ValueType, Is.EqualTo(PrimitiveTypes.Object));

            Assert.That(node.ChildNodes
                .Where(x => x.Name.Equals("nestedObject")).First().ChildNodes
                .Where(x => x.Name.Equals("nameValue")).First().As<JsonNode>().Value, Is.EqualTo("name1"));
            Assert.That(node.ChildNodes
                .Where(x => x.Name.Equals("nestedObject")).First().ChildNodes
                .Where(x => x.Name.Equals("subObject")).First().As<JsonNode>().ValueType, Is.EqualTo(PrimitiveTypes.Object));

            var subObjectNode = sutNode.ChildNodes
                .Where(x => x.Name.Equals("subObject")).First().ChildNodes
                .Where(x => x.Name.Equals("subObjectNameValue")).First().As<JsonNode>();
            Assert.That(subObjectNode.Value, Is.EqualTo("subObjectName1"));
            Assert.That(subObjectNode.ValueType, Is.EqualTo(PrimitiveTypes.String));

            subObjectNode = sutNode.ChildNodes
                .Where(x => x.Name.Equals("subObject")).First().ChildNodes
                .Where(x => x.Name.Equals("subObjectIdValue")).First().As<JsonNode>();
            Assert.That(subObjectNode.ValueType, Is.EqualTo(PrimitiveTypes.Integer));
            Assert.That(subObjectNode.Value, Is.EqualTo("201"));

            // validate array of simple objects
            sutNode = node.ChildNodes.Where(x => x.Name.Equals("myArrayOfObjects")).First().As<JsonNode>();
            Assert.That(sutNode.ValueType, Is.EqualTo(PrimitiveTypes.Array));
            Assert.That(sutNode.ChildNodes.Count, Is.EqualTo(3));
            Assert.That(sutNode.ChildNodes.First().ChildNodes
                .Where(y => y.Name.Equals("objname")).First().As<JsonNode>().Value, Is.EqualTo("object1"));
        }

        [Test]
        public void ShouldParseJsonTypes()
        {
            var responseJson = LoadTestData("AllTypesResponse.json");
            var node = _parser.Parse(responseJson);

            Assert.That(node.NodeType, Is.EqualTo(JsonNodeType.Object));
            Assert.That(node.ParentNode, Is.Null); // ensure we are at the root
            Assert.That(node.ChildNodes.Where(x => x.Name.Equals("intValue")).First().As<JsonNode>().ValueType, Is.EqualTo(PrimitiveTypes.Integer));
            Assert.That(node.ChildNodes.Where(x => x.Name.Equals("stringValue")).First().As<JsonNode>().ValueType, Is.EqualTo(PrimitiveTypes.String));
            Assert.That(node.ChildNodes.Where(x => x.Name.Equals("boolValue")).First().As<JsonNode>().ValueType, Is.EqualTo(PrimitiveTypes.Boolean));
            Assert.That(node.ChildNodes.Where(x => x.Name.Equals("numberValue")).First().As<JsonNode>().ValueType, Is.EqualTo(PrimitiveTypes.Number));
            Assert.That(node.ChildNodes.Where(x => x.Name.Equals("objectValue")).First().As<JsonNode>().ValueType, Is.EqualTo(PrimitiveTypes.Object));
            Assert.That(node.ChildNodes.Where(x => x.Name.Equals("arrayValue")).First().As<JsonNode>().ValueType, Is.EqualTo(PrimitiveTypes.Array));
            Assert.That(node.ChildNodes.Where(x => x.Name.Equals("nullValue")).First().As<JsonNode>().ValueType, Is.EqualTo(PrimitiveTypes.Null));
        }

        private string LoadTestData(string name)
        {
            return LoadResource(".ParsingData." + name);
        }

        private string LoadResource(string resourceName)
        {
            var txt = "";
            try
            {
                using (var stream = _assembly.GetManifestResourceStream(_assembly.GetName().Name + resourceName))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        txt = reader.ReadToEnd();
                    }
                }
            }
            catch (ArgumentNullException)
            {
                // invalid resource filename specified
                throw new Exception(String.Format("The resource name '{1}{0}' was not found.", resourceName, _assembly.GetName().Name));
            }
            return txt;
        }

        [SetUp]
        public void Setup()
        {
            _assembly = Assembly.GetExecutingAssembly();
            _parser = new JsonParser();
        }

        [TearDown]
        public void TeardownValidators()
        {

        }
    }
}
