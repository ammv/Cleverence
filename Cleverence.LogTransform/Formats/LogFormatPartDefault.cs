using Cleverence.LogTransform.Models;
using Cleverence.LogTransform.Utils;

namespace Cleverence.LogTransform.Formats
{
    /// <summary>
    /// Provides a default implementation of <see cref="ILogFormatPart"/> using delegate-based parsing and formatting logic.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class wraps user-provided parser and formatter functions into safe <see cref="TryFunc{T, TResult}"/> instances
    /// that never throw exceptions. If the custom formatter is omitted, a default formatter is used that calls
    /// <see cref="object.ToString()"/> and returns an empty string for <see langword="null"/> values.
    /// </para>
    /// <para>
    /// Instances are immutable and thread-safe.
    /// </para>
    /// </remarks>
    public class LogFormatPartDefault : ILogFormatPart
    {
        private static readonly TryFunc<object?, string> _defaultFormatter =
            new TryFunc<object?, string>(x => x?.ToString() ?? string.Empty);

        /// <summary>
        /// Gets the semantic type of this log part.
        /// </summary>
        public LogFormatPartType Type { get; }

        /// <summary>
        /// Gets the parser that converts a raw string to a typed value.
        /// </summary>
        /// <remarks>
        /// The underlying parser function is wrapped to catch all exceptions and return <see langword="false"/> on failure.
        /// </remarks>
        public TryFunc<string, object?> Parser { get; }

        /// <summary>
        /// Gets the formatter that converts a typed value to its string representation.
        /// </summary>
        /// <remarks>
        /// If no custom formatter was provided during construction, this uses a default implementation
        /// that returns <see cref="string.Empty"/> for <see langword="null"/> inputs and calls <see cref="object.ToString()"/> otherwise.
        /// The formatter is wrapped to catch all exceptions and return <see langword="false"/> on failure.
        /// </remarks>
        public TryFunc<object?, string> Formatter { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="LogFormatPartDefault"/>.
        /// </summary>
        /// <param name="type">The semantic type of the log part.</param>
        /// <param name="parser">
        /// A function that converts a string to a typed value. Must not be <see langword="null"/>.
        /// Exceptions thrown by this function will be caught and converted to a <see langword="false"/> result.
        /// </param>
        /// <param name="formatter">
        /// An optional function that converts a typed value to a string.
        /// If omitted, a default formatter is used.
        /// Exceptions thrown by this function will be caught and converted to a <see langword="false"/> result.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="parser"/> is <see langword="null"/>.
        /// </exception>
        public LogFormatPartDefault(
            LogFormatPartType type,
            Func<string, object?> parser,
            Func<object?, string>? formatter = null)
        {
            Type = type;

            ArgumentNullException.ThrowIfNull(parser, nameof(parser));

            Parser = new TryFunc<string, object?>(parser);

            if (formatter == null)
            {
                Formatter = _defaultFormatter;
            }
            else
            {
                Formatter = new TryFunc<object?, string>(formatter);
            }
        }
    }
}
