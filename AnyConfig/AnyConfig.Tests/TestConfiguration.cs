namespace AnyConfig.Tests
{
    public class TestConfiguration
    {
        public bool BoolSetting { get; set; }
        public string StringSetting { get; set; }
        public int IntSetting { get; set; }
        public CustomEnum CustomEnumSetting { get; set; }
        public CustomEnum CustomEnumNumericSetting { get; set; }

        [LegacyConfigurationName(prependChildrenName: "TestConfigurationObject", childrenMapped: true)]
        public TestConfigurationObject TestConfigurationObject { get; set; }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var typedObj = obj as TestConfiguration;
            if (typedObj is null)
                return false;

            return typedObj.BoolSetting.Equals(BoolSetting)
                && typedObj.StringSetting.Equals(StringSetting)
                && typedObj.IntSetting.Equals(IntSetting)
                && typedObj.CustomEnumSetting.Equals(CustomEnumSetting)
                && typedObj.CustomEnumNumericSetting.Equals(CustomEnumNumericSetting)
                && typedObj.TestConfigurationObject.Equals(TestConfigurationObject);
        }

        public override string ToString()
        {
            return $"BoolSetting={BoolSetting},StringSetting={StringSetting},IntSetting={IntSetting},CustomEnumSetting={CustomEnumSetting},CustomEnumNumericSetting={CustomEnumNumericSetting},\"TestConfigurationObject={TestConfigurationObject}\"";
        }
    }

    public class TestConfigurationObject
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var typedObj = obj as TestConfigurationObject;
            if (typedObj is null)
                return false;

            return typedObj.Name.Equals(Name) && typedObj.Value.Equals(Value);
        }

        public override string ToString()
        {
            return $"Name={Name},Value={Value}";
        }
    }

    public enum CustomEnum
    {
        First = 1,
        Second = 2,
        Third = 3
    }
}
