using Cleverence.LogTransform.Formats;
using Cleverence.LogTransform.Models;

namespace Cleverence.LogTransform.Transformation
{
    /// <summary>
    /// Represents a transformer that converts a <see cref="LogEntry"/> from a specific input format to another format.
    /// The transformer is bound to exactly one expected input format and validates the input at runtime.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class ensures type safety by verifying that the input log entry matches the expected format
    /// before applying the transformation logic.
    /// </para>
    /// <para>
    /// Instances are immutable and thread-safe.
    /// </para>
    /// </remarks>
    public sealed class FormatBoundLogTransformer
    {
        private readonly ILogFormat _expectedInputFormat;
        private readonly Func<LogEntry, LogEntry> _transform;

        /// <summary>
        /// Gets the log format that this transformer expects as input.
        /// </summary>
        public ILogFormat ExpectedInputFormat => _expectedInputFormat;

        private FormatBoundLogTransformer(ILogFormat expectedInputFormat, Func<LogEntry, LogEntry> transform)
        {
            _expectedInputFormat = expectedInputFormat;
            _transform = transform;
        }

        /// <summary>
        /// Transforms the specified log entry.
        /// </summary>
        /// <param name="input">The log entry to transform. Must not be <see langword="null"/>.</param>
        /// <returns>A new log entry in the target format.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown if the <see cref="LogEntry.Format"/> of <paramref name="input"/> does not match <see cref="ExpectedInputFormat"/>.
        /// </exception>
        /// <remarks>
        /// The transformation function must not return <see langword="null"/>.
        /// </remarks>
        public LogEntry Transform(LogEntry input)
        {
            if (input.Format != _expectedInputFormat)
            {
                throw new ArgumentException(
                    $"Input LogEntry has format '{input.Format}', but expected '{_expectedInputFormat}'.");
            }
            return _transform(input);
        }

        /// <summary>
        /// Creates a new instance of <see cref="FormatBoundLogTransformer"/>.
        /// </summary>
        /// <param name="expectedInputFormat">
        /// The input log format that this transformer is designed to handle. Must not be <see langword="null"/>.
        /// </param>
        /// <param name="transform">
        /// The transformation function that converts an input <see cref="LogEntry"/> to an output <see cref="LogEntry"/>.
        /// Must not be <see langword="null"/>.
        /// </param>
        /// <returns>A new <see cref="FormatBoundLogTransformer"/> instance.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if either <paramref name="expectedInputFormat"/> or <paramref name="transform"/> is <see langword="null"/>.
        /// </exception>
        public static FormatBoundLogTransformer Create(
            ILogFormat expectedInputFormat,
            Func<LogEntry, LogEntry> transform)
        {
            ArgumentNullException.ThrowIfNull(expectedInputFormat, nameof(expectedInputFormat));
            ArgumentNullException.ThrowIfNull(transform, nameof(transform));
            return new FormatBoundLogTransformer(expectedInputFormat, transform);
        }
    }
}
