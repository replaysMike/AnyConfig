using System;

namespace AnyConfig.Exceptions
{
    public class ParseException : Exception
    {
        public ParseException(string message) : base(message) { }
    }
}
