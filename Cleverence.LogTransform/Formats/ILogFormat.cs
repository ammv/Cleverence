namespace Cleverence.LogTransform.Formats
{
    /// <summary>
    /// Defines the contract for a structured log format capable of parsing raw log lines into typed fields.
    /// </summary>
    /// <remarks>
    /// <para>
    /// An <see cref="ILogFormat"/> describes how a textual log line is structured:
    /// it defines field names, their semantic types, parsing logic, and the character that separates fields (if applicable).
    /// </para>
    /// <para>
    /// Implementations must be immutable and thread-safe, as instances are often shared across components
    /// (e.g., used as keys in dictionaries or passed to parsers and transformers).
    /// </para>
    /// <para>
    /// The order of <see cref="PartNames"/> is significant and typically reflects the order of fields in the log line.
    /// </para>
    /// </remarks>
    public interface ILogFormat
    {
        /// <summary>
        /// Gets the character used to separate fields in the log line.
        /// </summary>
        /// <remarks>
        /// For non-delimited formats (e.g., regex-based), this character may be used only for formatting output,
        /// not for parsing.
        /// </remarks>
        char Separator { get; }

        /// <summary>
        /// Gets the ordered list of part names defined by this log format.
        /// </summary>
        /// <remarks>
        /// The order must be preserved and match the sequence of fields in the source log line.
        /// This list is read-only and must not be modified by the caller.
        /// </remarks>
        IReadOnlyList<string> PartNames { get; }

        /// <summary>
        /// Gets the log format part associated with the specified name.
        /// </summary>
        /// <param name="name">The name of the part to retrieve.</param>
        /// <returns>The <see cref="ILogFormatPart"/> instance for the given name.</returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown if no part with the specified <paramref name="name"/> exists.
        /// </exception>
        ILogFormatPart this[string name] { get; }

        /// <summary>
        /// Attempts to parse a raw log line into a dictionary of structured, typed values.
        /// </summary>
        /// <param name="log">The raw log line to parse. May be <see langword="null"/> or empty.</param>
        /// <param name="values">
        /// When this method returns, contains a read-only dictionary mapping part names to parsed values
        /// if parsing succeeded; otherwise, <see langword="null"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the log line was successfully parsed and all field parsers succeeded;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// Parsing logic is format-specific. For example:
        /// <list type="bullet">
        ///   <item><description><see cref="DelimetedLogFormat"/> splits the line by <see cref="Separator"/>.</description></item>
        ///   <item><description><see cref="RegexLogFormat"/> matches the line against a regular expression.</description></item>
        /// </list>
        /// Individual field parsers (exposed via <see cref="ILogFormatPart.Parser"/>) are responsible for
        /// converting raw strings to typed objects. If any parser fails, the entire parse operation fails.
        /// </remarks>
        bool TryParse(string log, out IReadOnlyDictionary<string, object?> values);
    }
}
