using System;
using System.Collections.Generic;
using System.Text;

namespace AnyConfig
{
    public enum RuntimeFramework
    {
        /// <summary>
        /// Runtime framework not detected
        /// </summary>
        Unknown,
        /// <summary>
        /// .Net Core
        /// </summary>
        DotNetCore,
        /// <summary>
        /// .Net Framework
        /// </summary>
        DotNetFramework
    }
}
