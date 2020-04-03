namespace AnyConfig.Json
{
    public interface IJsonParser
    {
        /// <summary>
        /// The original json string
        /// </summary>
        string Json { get; }

        /// <summary>
        /// True if this json is valid
        /// </summary>
        bool IsValidJson { get; }

        /// <summary>
        /// Parse a json string
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        JsonNode Parse(string json);
    }
}