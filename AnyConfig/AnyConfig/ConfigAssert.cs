using System.Text;
using System;

#if DEBUG
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("AnyConfig.Tests")]
#endif

namespace AnyConfig
{
    /// <summary>
    /// Writes assertions to the config buffer
    /// </summary>
    public static class ConfigAssert
    {
        private const string Prepend = "TEST: ";
        internal static StringBuilder _stringBuilder = new StringBuilder();
        internal static StringBuilder _conditionalBuilder = new StringBuilder();
        internal static Action<string> Target { get; set; } = Console.WriteLine;
        internal static int Length => _stringBuilder.Length;

        public static void WriteLine(string str)
        {
#if DEBUG
            _stringBuilder.AppendLine(Prepend + str);
#endif
        }

        public static void WriteLineConditional(string str)
        {
#if DEBUG
            _conditionalBuilder.AppendLine(str);
#endif
        }

        public static void FlushToConsole()
        {
#if DEBUG
            if (Length > 0)
                Target.Invoke(_conditionalBuilder.ToString());
            Target.Invoke(_stringBuilder.ToString());
            Reset();
#endif
        }

        public static void Reset()
        {
#if DEBUG
            _stringBuilder.Clear();
            _conditionalBuilder.Clear();
#endif
        }
    }
}
