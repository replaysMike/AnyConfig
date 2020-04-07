using AnyConfig.Xml;
using NUnit.Framework;
using System.Linq;

namespace AnyConfig.Tests.Xml
{
    [TestFixture]
    public class XmlSerializerTests
    {
        [Test]
        public void Serialize_ShouldSerializeType()
        {
            var xml = $@"<TestConfiguration>
    <BoolSetting>true</BoolSetting>
    <StringSetting>Testing value</StringSetting>
    <IntSetting>1234</IntSetting>
    <CustomEnumSetting>Second</CustomEnumSetting>
    <CustomEnumNumericSetting>2</CustomEnumNumericSetting>
    <TestConfigurationObject>
        <Name>TestName</Name>
        <Value>TestValue</Value>
    </TestConfigurationObject>
</TestConfiguration>";
            var obj = XmlSerializer.Deserialize<TestConfiguration>(xml);
            Assert.NotNull(obj);
            Assert.AreEqual(true, obj.BoolSetting);
            Assert.AreEqual("Testing value", obj.StringSetting);
            Assert.AreEqual(1234, obj.IntSetting);
            Assert.AreEqual(CustomEnum.Second, obj.CustomEnumSetting);
            Assert.AreEqual(CustomEnum.Second, obj.CustomEnumNumericSetting);
            Assert.AreEqual("TestName", obj.TestConfigurationObject.Name);
            Assert.AreEqual("TestValue", obj.TestConfigurationObject.Value);
        }

        [Test]
        public void Serialize_ShouldSerializeArrayOfType()
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
            var obj = XmlSerializer.Deserialize<MultipleConfiguration>(xml);
            Assert.NotNull(obj);
            Assert.AreEqual(2, obj.TestConfigurations.Count);
            Assert.AreEqual(true, obj.TestConfigurations.First().BoolSetting);
            Assert.AreEqual(1234, obj.TestConfigurations.First().IntSetting);
            Assert.AreEqual("Testing value", obj.TestConfigurations.First().StringSetting);
            Assert.AreEqual(false, obj.TestConfigurations.Skip(1).First().BoolSetting);
            Assert.AreEqual(5678, obj.TestConfigurations.Skip(1).First().IntSetting);
            Assert.AreEqual("Testing value 2", obj.TestConfigurations.Skip(1).First().StringSetting);
        }
    }
}
