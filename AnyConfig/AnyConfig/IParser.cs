namespace AnyConfig
{
    public interface IParser<TNode>
    {
        /// <summary>
        /// The original text to be parsed
        /// </summary>
        string OriginalText { get; }

        /// <summary>
        /// True if format is valid
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// Parse a string
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        TNode Parse(string text);
    }
}
