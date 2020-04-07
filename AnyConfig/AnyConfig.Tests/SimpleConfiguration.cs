namespace AnyConfig.Tests
{
    public class SimpleConfiguration
    {
        public bool BoolSetting { get; set; }
        public string StringSetting { get; set; }
        public int IntSetting { get; set; }
        public ChildSimpleConfiguration Child { get; set; }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var typedObj = obj as SimpleConfiguration;
            if (typedObj is null)
                return false;

            return typedObj.BoolSetting.Equals(BoolSetting)
                && typedObj.StringSetting.Equals(StringSetting)
                && typedObj.IntSetting.Equals(IntSetting)
                && typedObj.Child.Equals(Child);
        }

        public override string ToString()
        {
            return $"BoolSetting={BoolSetting},StringSetting={StringSetting},IntSetting={IntSetting},\"Child={Child}\"";
        }
    }

    public class ChildSimpleConfiguration
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var typedObj = obj as ChildSimpleConfiguration;
            if (typedObj is null)
                return false;

            return typedObj.Name.Equals(Name) && typedObj.Value.Equals(Value);
        }

        public override string ToString()
        {
            return $"Name={Name},Value={Value}";
        }
    }
}
