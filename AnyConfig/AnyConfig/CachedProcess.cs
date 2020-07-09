using System;
using System.Diagnostics;

namespace AnyConfig
{
    /// <summary>
    /// Cache wrapper for the current process
    /// </summary>
    public class CachedProcess : Process
    {
        private static Lazy<string> _currentProcessFilename = new Lazy<string>(() => _currentProcess.Value.GetProcessPath());
        private static Lazy<Process> _currentProcess = new Lazy<Process>(() => Process.GetCurrentProcess());

        /// <summary>
        /// Get the current process (cached)
        /// </summary>
        /// <returns></returns>
        public static new Process GetCurrentProcess() => _currentProcess.Value;

        /// <summary>
        /// Get the current process filename (cached)
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentProcessFilename()
            => _currentProcessFilename.Value;
    }
}
