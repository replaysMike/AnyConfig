using System;
using System.Collections.Generic;
using System.Text;

namespace AnyConfig
{
    /// <summary>
    /// Multi-framework configuration resolver
    /// </summary>
    public interface IConfigurationResolver
    {
        /// <summary>
        /// The detected runtime framework
        /// </summary>
        RuntimeFramework DetectedRuntimeFramework { get; }

        /// <summary>
        /// The detected OS platform
        /// </summary>
        string DetectedRuntimePlatform { get; }

        /// <summary>
        /// The detected runtime framework as per OS
        /// </summary>
        string DetectedRuntimeFrameworkDescription { get; }

        /// <summary>
        /// Resolve a configuration for the current runtime platform
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T ResolveConfiguration<T>();
    }
}
