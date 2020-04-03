namespace AnyConfig.Tests
{
    public class TestConfiguration
    {
        public bool BoolSetting { get; set; }
        public string StringSetting { get; set; }
        public int IntSetting { get; set; }

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
                && typedObj.TestConfigurationObject.Equals(TestConfigurationObject);
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
    }
}
