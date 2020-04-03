namespace AnyConfig.Json
{
    // 3.5 JSON Schema Primitive Types
    public enum PrimitiveTypes
    {
        Null = 0,
        Array = 1,
        Boolean = 2,
        Integer = 3, /* when testing for equality, integers can not have decimals */
        Number = 4,
        Object = 5,
        String = 6,
        Reference = 7
    }
}
