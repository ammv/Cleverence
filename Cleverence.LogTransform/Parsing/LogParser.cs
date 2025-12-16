using Cleverence.LogTransform.Exceptions;
using Cleverence.LogTransform.Formats;
using Cleverence.LogTransform.Models;
using System.Diagnostics.CodeAnalysis;

namespace Cleverence.LogTransform.Parsing
{
    /// <summary>
    /// Parses raw log lines into structured <see cref="LogEntry"/> instances using a set of registered log formats.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class attempts to parse an input line by trying each registered <see cref="ILogFormat"/> in sequence
    /// until one succeeds. The first matching format is used to produce the resulting <see cref="LogEntry"/>.
    /// </para>
    /// <para>
    /// If parsing fails for all registered formats, a <see cref="LogParseException"/> is thrown (in <see cref="Parse"/>)
    /// or <see langword="false"/> is returned (in <see cref="TryParse"/>).
    /// </para>
    /// <para>
    /// This class is stateless and thread-safe.
    /// </para>
    /// </remarks>
    public sealed class LogParser
    {
        private readonly IReadOnlyList<ILogFormat> _supportedFormats;

        /// <summary>
        /// Initializes a new instance of <see cref="LogParser"/> with the specified log formats.
        /// </summary>
        /// <param name="supportedFormats">
        /// A non-empty list of unique, immutable log format definitions. Must not be <see langword="null"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="supportedFormats"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="supportedFormats"/> is empty.
        /// </exception>
        public LogParser(IReadOnlyList<ILogFormat> supportedFormats)
        {
            ArgumentNullException.ThrowIfNull(supportedFormats, nameof(supportedFormats));
            if (supportedFormats.Count == 0)
                throw new ArgumentException("At least one log format must be provided.", nameof(supportedFormats));

            _supportedFormats = supportedFormats;
        }

        /// <summary>
        /// Parses a log line using the first matching supported format.
        /// </summary>
        /// <param name="logLine">The raw log line to parse. Must not be <see langword="null"/> or empty.</param>
        /// <returns>A structured <see cref="LogEntry"/> instance.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="logLine"/> is <see langword="null"/> or empty.
        /// </exception>
        /// <exception cref="LogParseException">
        /// Thrown if no registered format can successfully parse the line.
        /// The exception message includes a truncated version of the input for diagnostics.
        /// </exception>
        public LogEntry Parse(string logLine)
        {
            ArgumentException.ThrowIfNullOrEmpty(logLine, nameof(logLine));

            if (TryParse(logLine, out var entry))
                return entry;

            throw new LogParseException($"Failed to parse log line: \"{Truncate(logLine, 100)}\". " +
                                       $"No registered format matches the input.");
        }

        /// <summary>
        /// Attempts to parse a log line using the supported formats.
        /// </summary>
        /// <param name="logLine">The raw log line to parse.</param>
        /// <param name="logEntry">
        /// When this method returns, contains the resulting <see cref="LogEntry"/> if parsing succeeded;
        /// otherwise, <see langword="null"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if parsing succeeded using one of the registered formats;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// This method returns <see langword="false"/> for <see langword="null"/> or empty input,
        /// and when none of the registered formats can parse the line.
        /// </remarks>
        public bool TryParse(string logLine, [NotNullWhen(true)] out LogEntry? logEntry)
        {
            logEntry = null;

            if (string.IsNullOrEmpty(logLine))
                return false;

            foreach (var format in _supportedFormats)
            {
                if (format.TryParse(logLine, out var values))
                {
                    logEntry = new LogEntry(format, values);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Truncates a string to a maximum length, appending an ellipsis if shortened.
        /// </summary>
        /// <param name="value">The string to truncate.</param>
        /// <param name="maxLength">The maximum allowed length of the result.</param>
        /// <returns>
        /// The original string if its length is within <paramref name="maxLength"/>;
        /// otherwise, a truncated string ending with "...".
        /// </returns>
        private static string Truncate(string value, int maxLength)
        {
            if (value.Length <= maxLength) return value;
            return value.Substring(0, maxLength - 3) + "...";
        }
    }
}
