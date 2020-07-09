using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace AnyConfig
{
    public static class ProcessExtensions
    {
        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern bool QueryFullProcessImageName([In] IntPtr hProcess, [In] uint dwFlags, [Out] StringBuilder lpExeName, [In, Out] ref uint lpdwSize);

        /// <summary>
        /// Get the path of the current process
        /// </summary>
        /// <param name="process"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static string GetProcessPath(this Process process, int buffer = 1024)
        {
            var fileNameBuilder = new StringBuilder(buffer);
            uint bufferLength = (uint)fileNameBuilder.Capacity + 1;
            return QueryFullProcessImageName(process.Handle, 0, fileNameBuilder, ref bufferLength) ?
                fileNameBuilder.ToString() :
                null;
        }
    }
}
