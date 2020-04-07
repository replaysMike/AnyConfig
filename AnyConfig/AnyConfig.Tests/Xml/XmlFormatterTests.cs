using AnyConfig.Xml;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnyConfig.Tests.Xml
{
    [TestFixture]
    public class XmlFormatterTests
    {
        [Test]
        public void Should_Format_Xml()
        {
            var xml = $@"<?xml version=""1.0"" encoding=""utf-8"" ?><TestObject><!--Ignore me--><ChildObject name=""child1"" type=""ChildObject"">Inner text</ChildObject></TestObject>";
            var formatter = new XmlFormatter();
            var formattedXml = formatter.PrettifyXml(xml);

            var expectedFormattedXml = $@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<TestObject>
    <!--Ignore me-->
    <ChildObject name=""child1"" type=""ChildObject"">
        Inner text
    </ChildObject>
</TestObject>
";
            // fix line ending encoding on AppVeyor tests
            expectedFormattedXml = expectedFormattedXml.Replace("\r\n", "\n").Replace("\n", "\r\n");
            Assert.AreEqual(expectedFormattedXml, formattedXml);
        }
    }
}
