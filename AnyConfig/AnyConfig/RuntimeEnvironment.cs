using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace AnyConfig
{
    public static class RuntimeEnvironment
    {
        /// <summary>
        /// Detect the current runtime
        /// </summary>
        /// <returns></returns>
        public static RuntimeInfo DetectRuntime()
        {
            return DetectRuntime(null);
        }

        /// <summary>
        /// Detect the current runtime
        /// </summary>
        /// <param name="entryAssembly"></param>
        /// <returns></returns>
        public static RuntimeInfo DetectRuntime(Assembly entryAssembly)
        {
            var info = new RuntimeInfo();

            string framework = null;
            info.DetectedRuntimePlatform = RuntimeInformation.OSDescription;
            info.DetectedRuntimeFrameworkDescription = RuntimeInformation.FrameworkDescription;
            if (info.DetectedRuntimeFrameworkDescription.Contains(".NET Core"))
                info.DetectedRuntimeFramework = RuntimeFramework.DotNetCore;

#if NETFRAMEWORK
            if (entryAssembly == null)
            {
                var appDomain = AppDomain.CurrentDomain.SetupInformation;
                info.ConfigFile = appDomain.ConfigurationFile;
                info.DetectedRuntimeFramework = RuntimeFramework.DotNetFramework;
                framework = appDomain.TargetFrameworkName;
            }
#endif
            // if we have a known entry assembly, use that as it may be more reliable
            if (framework == null)
                framework = entryAssembly?.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName;
            if (framework?.Contains(".NETCoreApp") == true) // ".NETCoreApp,Version=v2.1"
                info.DetectedRuntimeFramework = RuntimeFramework.DotNetCore;
            if (framework?.Contains(".NETFramework") == true) // ".NETFramework,Version=v4.8"
                info.DetectedRuntimeFramework = RuntimeFramework.DotNetFramework;

            return info;
        }
    }
}
