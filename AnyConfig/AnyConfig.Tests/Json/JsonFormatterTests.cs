using AnyConfig.Json;
using NUnit.Framework;

namespace AnyConfig.Tests.Json
{
    [TestFixture]
    public class JsonFormatterTests
    {
        [Test]
        public void Should_Format_Json()
        {
            var json = $@"{{""TestConfiguration"": {{""BoolSetting"": true,""StringSetting"": ""Testing value"",""IntSetting"": 1234,""CustomEnumSetting"": ""Second"",""CustomEnumNumericSetting"": 2,""TestConfigurationObject"": {{""Name"": ""TestName"",""Value"": ""TestValue""}}}}}}";
            var formatter = new JsonFormatter();
            var formattedXml = formatter.PrettifyJson(json);

            var expectedFormattedJson = $@"{{
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
            Assert.AreEqual(expectedFormattedJson, formattedXml);
        }
    }
}
