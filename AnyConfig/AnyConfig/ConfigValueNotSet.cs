namespace AnyConfig
{
    public class ConfigValueNotSet
    {
        public static ConfigValueNotSet Instance => _instance;
        private static readonly ConfigValueNotSet _instance = new ConfigValueNotSet();
        static ConfigValueNotSet() { }
        private ConfigValueNotSet() { }
        public override string ToString()
        {
            return "Config value not set";
        }
    }
}
