namespace AnyConfig.Tests
{
    public class LegacySettingNameConfiguration
    {
        [LegacyConfigurationName(settingName: "CustomName")]
        public string AlternateName { get; set; }
    }
}
