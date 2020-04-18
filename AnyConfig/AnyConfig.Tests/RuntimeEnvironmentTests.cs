using NUnit.Framework;

namespace AnyConfig.Tests
{
    [TestFixture]
    public class RuntimeEnvironmentTests
    {
        [Test]
        public void Should_Detect_Environment()
        {
            var detectedRuntime = RuntimeEnvironment.DetectRuntime();
#if NETFRAMEWORK
            Assert.AreEqual(RuntimeFramework.DotNetFramework, detectedRuntime.DetectedRuntimeFramework);
#elif NETCOREAPP
            Assert.AreEqual(RuntimeFramework.DotNetCore, detectedRuntime.DetectedRuntimeFramework);
#else
            Assert.Fail($"Unknown or unsupported framework");
#endif
        }
    }
}
