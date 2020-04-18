using AnyConfig.Collections;
using TypeSupport.Extensions;

namespace AnyConfig.Models
{
    public class LegacyConfigurationSection
    {
        private object _configuration;

        public string this[string key]
        {
            get
            {
                return _configuration?.GetPropertyValue(key).ToString();
            }
            set
            {
                // nothing to set
            }
        }

        public Configuration CurrentConfiguration { get; }

        public SectionInformation SectionInformation { get; } = new SectionInformation();

        public LegacyConfigurationSection(Configuration currentConfiguration, SectionCollection configuration)
        {
            CurrentConfiguration = currentConfiguration;
            SectionInformation = configuration.SectionInformation;
            _configuration = configuration;
        }

        public LegacyConfigurationSection(Configuration currentConfiguration, object configuration)
        {
            CurrentConfiguration = currentConfiguration;
            if (configuration is SectionCollection)
                SectionInformation = ((SectionCollection)configuration).SectionInformation;
            _configuration = configuration;
        }

        public void DeserializeSection(string xml)
        {

        }
    }
}
