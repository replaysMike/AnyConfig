using System.Collections.Concurrent;
using System.IO;

namespace AnyConfig
{
    internal class CachedFileProvider : CachedDataProvider<string>
    {
        internal static ConcurrentDictionary<string, string> _cachedConfigurations = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// Add file to cache and return its contents, or get cached file contents
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        internal string AddOrGetFile(string filename)
        {
            return AddOrGet(filename, () =>
            {
                if (!File.Exists(filename))
                    throw new FileNotFoundException($"The configuration file named '{filename}' was not found.");
                return File.ReadAllText(filename);

            }) as string;
        }
    }
}
