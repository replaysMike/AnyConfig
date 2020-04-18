namespace AnyConfig.Collections
{
    public interface IKeyable<T> : IKeyable
    {
        new string Key { get; }
        void Set(T value);
    }

    public interface IKeyable
    {
        string Key { get; }
        void Set(object value);
    }
}
