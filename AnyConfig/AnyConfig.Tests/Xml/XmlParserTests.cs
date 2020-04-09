using AnyConfig.Xml;
using NUnit.Framework;
using System.Linq;

namespace AnyConfig.Tests.Xml
{
    [TestFixture]
    public class XmlParserTests
    {
        [Test]
        public void Should_Parse_Xml()
        {
            var xml = $@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<TestObject>
<!--Ignore me-->
<ChildObject name=""child1"" type=""ChildObject"">Inner text</ChildObject>
</TestObject>";
            var parser = new XmlParser();
            var node = parser.Parse(xml);

            Assert.NotNull(node);
            Assert.NotNull(node.DeclarationNode);
            Assert.AreEqual("Declaration", node.DeclarationNode.Name);
            Assert.AreEqual(1, node.ChildNodes.Count);
            Assert.AreEqual("TestObject", node.Name);
            Assert.AreEqual("/TestObject", node.FullPath);
            Assert.AreEqual(0, node.Attributes.Count);
            Assert.AreEqual("/TestObject/ChildObject", node.ChildNodes.First().FullPath);
            Assert.AreEqual("ChildObject", node.ChildNodes.First().Name);
            Assert.AreEqual(2, node.ChildNodes.First().As<XmlNode>().Attributes.Count);
        }

        [Test]
        public void Should_Parse_Xml_WithoutDeclaration()
        {
            var xml = $@"<TestObject>
<!--Ignore me-->
<ChildObject name=""child1"" type=""ChildObject"">Inner text</ChildObject>
</TestObject>";
            var parser = new XmlParser();
            var node = parser.Parse(xml);

            Assert.NotNull(node);
            Assert.IsNull(node.DeclarationNode);
            Assert.AreEqual(1, node.ChildNodes.Count);
            Assert.AreEqual("TestObject", node.Name);
            Assert.AreEqual("/TestObject", node.FullPath);
            Assert.AreEqual(0, node.Attributes.Count);
            Assert.AreEqual("/TestObject/ChildObject", node.ChildNodes.First().FullPath);
            Assert.AreEqual("ChildObject", node.ChildNodes.First().Name);
            Assert.AreEqual(2, node.ChildNodes.First().As<XmlNode>().Attributes.Count);
        }

        [Test]
        public void Should_Parse_Xml_WithShortEndTag()
        {
            var xml = $@"<TestObject>
<!--Ignore me-->
<ChildObject test=""123"" />
</TestObject>";
            var parser = new XmlParser();
            var node = parser.Parse(xml);

            Assert.NotNull(node);
            Assert.IsNull(node.DeclarationNode);
            Assert.AreEqual(1, node.ChildNodes.Count);
            Assert.AreEqual("TestObject", node.Name);
            Assert.AreEqual("/TestObject", node.FullPath);
            Assert.AreEqual(0, node.Attributes.Count);
            Assert.AreEqual("/TestObject/ChildObject", node.ChildNodes.First().FullPath);
            Assert.AreEqual("ChildObject", node.ChildNodes.First().Name);
            Assert.AreEqual(1, node.ChildNodes.First().As<XmlNode>().Attributes.Count);
        }

        [Test]
        public void Should_Parse_Xml_ShouldDetectAttributes()
        {
            var xml = $@"<TestObject>
<ChildObject test=""123"" />
</TestObject>";
            var parser = new XmlParser();
            var node = parser.Parse(xml);

            Assert.NotNull(node);
            Assert.IsNull(node.DeclarationNode);
            Assert.AreEqual(1, node.ChildNodes.Count);
            Assert.AreEqual("TestObject", node.Name);
            Assert.AreEqual("/TestObject", node.FullPath);
            Assert.AreEqual(0, node.Attributes.Count);
            Assert.AreEqual("/TestObject/ChildObject", node.ChildNodes.First().FullPath);
            Assert.AreEqual("ChildObject", node.ChildNodes.First().Name);
            Assert.AreEqual(1, node.ChildNodes.First().As<XmlNode>().Attributes.Count);
        }

        [Test]
        public void Should_Parse_Xml_ShouldNotDetectAttributes()
        {
            var xml = $@"<TestObject>
<ChildObject />
</TestObject>";
            var parser = new XmlParser();
            var node = parser.Parse(xml);

            Assert.NotNull(node);
            Assert.IsNull(node.DeclarationNode);
            Assert.AreEqual(1, node.ChildNodes.Count);
            Assert.AreEqual("TestObject", node.Name);
            Assert.AreEqual("/TestObject", node.FullPath);
            Assert.AreEqual(0, node.Attributes.Count);
            Assert.AreEqual("/TestObject/ChildObject", node.ChildNodes.First().FullPath);
            Assert.AreEqual("ChildObject", node.ChildNodes.First().Name);
            Assert.AreEqual(0, node.ChildNodes.First().As<XmlNode>().Attributes.Count);
        }

        [Test]
        public void Should_Parse_Xml_WhitespaceIgnoredOnShortTags()
        {
            var xml = $@"<TestObject>
<ChildObject/>
</TestObject>";
            var parser = new XmlParser();
            var node = parser.Parse(xml);

            Assert.NotNull(node);
            Assert.IsNull(node.DeclarationNode);
            Assert.AreEqual(1, node.ChildNodes.Count);
            Assert.AreEqual("TestObject", node.Name);
            Assert.AreEqual("/TestObject", node.FullPath);
            Assert.AreEqual(0, node.Attributes.Count);
            Assert.AreEqual("/TestObject/ChildObject", node.ChildNodes.First().FullPath);
            Assert.AreEqual("ChildObject", node.ChildNodes.First().Name);
            Assert.AreEqual(0, node.ChildNodes.First().As<XmlNode>().Attributes.Count);
        }

        [Test]
        public void Should_Parse_Xml_Arrays()
        {
            var xml = $@"<MultipleConfiguration>
    <TestConfigurations>
        <TestConfiguration>
            <BoolSetting>true</BoolSetting>
            <StringSetting>Testing value</StringSetting>
            <IntSetting>1234</IntSetting>
            <CustomEnumSetting>Second</CustomEnumSetting>
            <CustomEnumNumericSetting>2</CustomEnumNumericSetting>
            <TestConfigurationObject>
                <Name>TestName</Name>
                <Value>TestValue</Value>
            </TestConfigurationObject>
        </TestConfiguration>
        <TestConfiguration>
            <BoolSetting>false</BoolSetting>
            <StringSetting>Testing value 2</StringSetting>
            <IntSetting>5678</IntSetting>
            <CustomEnumSetting>First</CustomEnumSetting>
            <CustomEnumNumericSetting>1</CustomEnumNumericSetting>
            <TestConfigurationObject>
                <Name>TestName 2</Name>
                <Value>TestValue 2</Value>
            </TestConfigurationObject>
        </TestConfiguration>
    </TestConfigurations>
</MultipleConfiguration>
";
            var parser = new XmlParser();
            var node = parser.Parse(xml);

            Assert.NotNull(node);
            Assert.AreEqual("MultipleConfiguration", node.Name);
            Assert.AreEqual("TestConfigurations", node.ChildNodes.First().Name);
            Assert.AreEqual(2, node.ChildNodes.First().ChildNodes.Count);
            Assert.AreEqual(2, node.ChildNodes.First().ArrayNodes.Count);
            Assert.AreEqual(0, node.ChildNodes.First().ArrayNodes.First().ArrayPosition);
            Assert.AreEqual("TestConfiguration", node.ChildNodes.First().ArrayNodes.First().Name);
            Assert.AreEqual("true", node.ChildNodes.First().ArrayNodes.First().ChildNodes.First().As<XmlNode>().InnerContent);
            Assert.AreEqual(1, node.ChildNodes.First().ArrayNodes.Skip(1).First().ArrayPosition);
            Assert.AreEqual("TestConfiguration", node.ChildNodes.First().ArrayNodes.Skip(1).First().Name);
            Assert.AreEqual("false", node.ChildNodes.First().ArrayNodes.Skip(1).First().ChildNodes.First().As<XmlNode>().InnerContent);
        }
    }
}
