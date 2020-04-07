using AnyConfig.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnyConfig.Tests.Json
{
    [TestFixture]
    public class JsonSerializerTests
    {
        [TestCase]
        public void ShouldSerialize_ShouldSerializeType()
        {
            var json = $@"{{
    ""TestConfiguration"": {{
        ""BoolSetting"": true,
        ""StringSetting"": ""Testing value"",
        ""IntSetting"": 1234,
        ""CustomEnumSetting"": ""Second"",
        ""CustomEnumNumericSetting"": 2,
        ""TestConfigurationObject"": {{
            ""Name"": ""TestName"",
            ""Value"": ""TestValue""
        }}
    }}
}}
";
            var obj = JsonSerializer.Deserialize<TestConfiguration>(json);
            Assert.NotNull(obj);
            Assert.AreEqual(true, obj.BoolSetting);
            Assert.AreEqual("Testing value", obj.StringSetting);
            Assert.AreEqual(1234, obj.IntSetting);
            Assert.AreEqual(CustomEnum.Second, obj.CustomEnumSetting);
            Assert.AreEqual(CustomEnum.Second, obj.CustomEnumNumericSetting);
            Assert.AreEqual("TestName", obj.TestConfigurationObject.Name);
            Assert.AreEqual("TestValue", obj.TestConfigurationObject.Value);
        }
    }
}
