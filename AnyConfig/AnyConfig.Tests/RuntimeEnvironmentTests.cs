using NUnit.Framework;

namespace AnyConfig.Tests
{
    [TestFixture]
    [NonParallelizable]
    public class RuntimeEnvironmentTests
    {
        [Test]
        public void Should_Detect_Environment()
        {
            var detectedRuntime = RuntimeEnvironment.DetectRuntime();
#if NETFRAMEWORK
            Assert.AreEqual(RuntimeFramework.DotNetFramework, detectedRuntime.DetectedRuntimeFramework);
#elif NET5_0
            Assert.AreEqual(RuntimeFramework.DotNet5, detectedRuntime.DetectedRuntimeFramework);
#elif NETCOREAPP
            Assert.AreEqual(RuntimeFramework.DotNetCore, detectedRuntime.DetectedRuntimeFramework);
#else
            Assert.Fail($"Unknown or unsupported framework");
#endif
        }
    }
}
