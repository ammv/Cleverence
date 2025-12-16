using Cleverence.LogTransform.Models;

namespace Cleverence.LogTransform.Formats.Builders
{
    /// <summary>
    /// Provides a fluent builder for constructing immutable <see cref="LogFormatPartSet"/> instances.
    /// Ensures that log format parts are registered with unique names and preserves the order of registration.
    /// </summary>
    /// <remarks>
    /// This builder enforces that:
    /// <list type="bullet">
    ///   <item><description>Each part name is unique within the set.</description></item>
    ///   <item><description>At least one part is added before building.</description></item>
    ///   <item><description>The order of parts matches the order of calls to <see cref="AddPart(string, ILogFormatPart)"/>.</description></item>
    /// </list>
    /// The resulting <see cref="LogFormatPartSet"/> is immutable and safe for concurrent use.
    /// </remarks>
    public sealed class LogFormatPartSetBuilder
    {
        private readonly Dictionary<string, ILogFormatPart> _parts = new();
        private readonly List<string> _partNamesOrdered = new();

        /// <summary>
        /// Adds a named log format part to the builder.
        /// </summary>
        /// <param name="name">The unique name of the log part. Must not be <see langword="null"/> or empty.</param>
        /// <param name="part">The log format part implementation. Must not be <see langword="null"/>.</param>
        /// <returns>The same builder instance to allow method chaining.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="name"/> or <paramref name="part"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if a part with the specified <paramref name="name"/> has already been added.
        /// </exception>
        public LogFormatPartSetBuilder AddPart(string name, ILogFormatPart part)
        {
            ArgumentNullException.ThrowIfNull(name, nameof(name));
            ArgumentNullException.ThrowIfNull(part, nameof(part));

            if (_parts.ContainsKey(name))
                throw new InvalidOperationException(
                    $"A log part with name '{name}' is already defined.");

            _parts.Add(name, part);
            _partNamesOrdered.Add(name);
            return this;
        }

        /// <summary>
        /// Adds a named log format part by specifying its type, parser, and optional formatter.
        /// Internally creates a <see cref="LogFormatPartDefault"/> instance.
        /// </summary>
        /// <param name="name">The unique name of the log part. Must not be <see langword="null"/> or empty.</param>
        /// <param name="type">The semantic type of the log part.</param>
        /// <param name="parser">A function that converts a string to a typed value. Must not be <see langword="null"/>.</param>
        /// <param name="formatter">An optional function that converts a typed value to a string. If omitted, a default formatter is used.</param>
        /// <returns>The same builder instance to allow method chaining.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="name"/> or <paramref name="parser"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if a part with the specified <paramref name="name"/> has already been added.
        /// </exception>
        public LogFormatPartSetBuilder AddPart(
            string name,
            LogFormatPartType type,
            Func<string, object?> parser,
            Func<object?, string>? formatter = null)
        {
            var part = new LogFormatPartDefault(type, parser, formatter);
            return AddPart(name, part);
        }

        /// <summary>
        /// Builds and returns an immutable <see cref="LogFormatPartSet"/> from the registered parts.
        /// </summary>
        /// <returns>A new <see cref="LogFormatPartSet"/> containing all added parts in registration order.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if no parts have been added to the builder.
        /// </exception>
        public LogFormatPartSet Build()
        {
            if (_parts.Count == 0)
                throw new InvalidOperationException("At least one log part must be added.");

            return new LogFormatPartSet(_partNamesOrdered, _parts.AsReadOnly());
        }
    }
}
