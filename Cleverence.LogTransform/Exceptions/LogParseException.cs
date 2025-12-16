namespace Cleverence.LogTransform.Exceptions
{
    /// <summary>
    /// Thrown when a log line cannot be parsed by any of the supported formats.
    /// </summary>
    public class LogParseException : Exception
    {
        public LogParseException(string message) : base(message) { }
        public LogParseException(string message, Exception innerException) : base(message, innerException) { }
    }
}
