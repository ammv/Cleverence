namespace Cleverence.LogTransform.Models
{
    /// <summary>
    /// Defines the semantic data type of a log format part.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This enumeration provides metadata about the expected content of a log field.
    /// It is used for:
    /// <list type="bullet">
    ///   <item>Validation during parsing and transformation.</item>
    ///   <item>Enabling type-safe accessors in <see cref="LogEntry"/> (e.g., <see cref="LogEntry.GetDateTimeValue"/>).</item>
    ///   <item>Guiding formatting and display logic in logging pipelines.</item>
    /// </list>
    /// </para>
    /// <para>
    /// The values do not enforce runtime type safety but serve as a contract between components.
    /// Actual storage type is always <see cref="object"/>, and casting is performed by the consumer.
    /// </para>
    /// </remarks>
    public enum LogFormatPartType
    {
        /// <summary>
        /// Represents a date and time value (typically stored as <see cref="DateTime"/>).
        /// </summary>
        DATETIME,

        /// <summary>
        /// Represents a 32-bit signed integer value (typically stored as <see cref="int"/>).
        /// </summary>
        INTEGER,

        /// <summary>
        /// Represents a floating-point numeric value (typically stored as <see cref="float"/> or <see cref="double"/>).
        /// </summary>
        FLOAT,

        /// <summary>
        /// Represents textual content (typically stored as <see cref="string"/>).
        /// </summary>
        TEXT,

        /// <summary>
        /// Represents a value that does not fit into other predefined categories.
        /// </summary>
        OTHER
    }
}
