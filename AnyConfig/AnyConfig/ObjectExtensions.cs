namespace AnyConfig
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// Returns true if null or <seealso cref="ConfigProvider.Empty"/>
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this object value) => value == null || value == ConfigProvider.Empty;
    }
}
