using Cleverence.LogTransform.Models;
using Cleverence.LogTransform.Utils;

namespace Cleverence.LogTransform.Formats
{
    /// <summary>
    /// Represents a single named component (field) within a structured log format.
    /// Encapsulates the semantic type, parsing, and formatting behavior for the field.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Each log format part defines how a raw string value is converted to a typed object (<see cref="Parser"/>)
    /// and how a typed object is converted back to a string (<see cref="Formatter"/>).
    /// </para>
    /// <para>
    /// Implementations must be immutable and thread-safe, as instances are typically shared across log entries
    /// and formatters.
    /// </para>
    /// </remarks>
    public interface ILogFormatPart
    {
        /// <summary>
        /// Gets the semantic type of the log part, which indicates the expected data kind
        /// (e.g., text, integer, date/time).
        /// </summary>
        /// <remarks>
        /// This type is used for validation, transformation, and user-facing diagnostics.
        /// It does not enforce runtime type safety but provides metadata for tooling and logic.
        /// </remarks>
        LogFormatPartType Type { get; }

        /// <summary>
        /// Gets a parser that safely converts a raw string value into a typed object.
        /// </summary>
        /// <remarks>
        /// The parser must encapsulate all format-specific parsing logic (e.g., date formats, number styles).
        /// It returns <see langword="false"/> if the input cannot be parsed, without throwing exceptions.
        /// </remarks>
        TryFunc<string, object?> Parser { get; }

        /// <summary>
        /// Gets a formatter that safely converts a typed object into its string representation.
        /// </summary>
        /// <remarks>
        /// The formatter must handle <see langword="null"/> inputs gracefully (unless the underlying type forbids it).
        /// It returns <see langword="false"/> if formatting fails, without throwing exceptions.
        /// </remarks>
        TryFunc<object?, string> Formatter { get; }
    }
}
