namespace AnyConfig
{
    public class RuntimeInfo
    {
        /// <summary>
        /// The detected runtime framework
        /// </summary>
        public RuntimeFramework DetectedRuntimeFramework { get; internal set; } = RuntimeFramework.DotNetFramework;

        /// <summary>
        /// The detected OS platform
        /// </summary>
        public string DetectedRuntimePlatform { get; internal set; }

        /// <summary>
        /// The detected runtime framework as per OS
        /// </summary>
        public string DetectedRuntimeFrameworkDescription { get; internal set; }

        /// <summary>
        /// Legacy config file location
        /// </summary>
        public string ConfigFile { get; internal set; }
    }
}
