using Cleverence.LogTransform.Formats;

namespace Cleverence.LogTransform.Models
{
    /// <summary>
    /// Represents a structured log entry with typed field access based on its associated log format.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A <see cref="LogEntry"/> is immutable and binds a set of field values to a specific <see cref="ILogFormat"/>.
    /// It provides both generic and type-safe accessors (e.g., <see cref="GetStringValue"/>, <see cref="GetIntegerValue"/>)
    /// that validate field existence and semantic type at runtime.
    /// </para>
    /// <para>
    /// All accessors enforce:
    /// <list type="bullet">
    ///   <item>That the requested field name exists in the format.</item>
    ///   <item>That the field's declared <see cref="LogFormatPartType"/> matches the expected type.</item>
    /// </list>
    /// </para>
    /// <para>
    /// This class is thread-safe and immutable.
    /// </para>
    /// </remarks>
    public sealed class LogEntry
    {
        /// <summary>
        /// Gets the log format that defines the structure of this entry.
        /// </summary>
        public ILogFormat Format { get; }

        /// <summary>
        /// Gets the dictionary of field values, keyed by part name.
        /// </summary>
        /// <remarks>
        /// The dictionary is read-only and contains exactly one value for each part defined in <see cref="Format"/>.
        /// </remarks>
        public IReadOnlyDictionary<string, object?> Values { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="LogEntry"/>.
        /// </summary>
        /// <param name="format">The log format that defines the expected structure. Must not be <see langword="null"/>.</param>
        /// <param name="values">A dictionary of field values. Must not be <see langword="null"/> and must contain exactly one entry for each part in <paramref name="format"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if either <paramref name="format"/> or <paramref name="values"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if the number of values does not match the number of parts in the format.
        /// </exception>
        public LogEntry(ILogFormat format, IReadOnlyDictionary<string, object?> values)
        {
            ArgumentNullException.ThrowIfNull(format, nameof(format));
            ArgumentNullException.ThrowIfNull(values, nameof(values));

            if (format.PartNames.Count != values.Count)
            {
                throw new ArgumentException(
                    $"The count of format parts ({format.PartNames.Count}) does not match the count of values ({values.Count}).",
                    nameof(values));
            }

            Format = format;
            Values = values;
        }

        /// <summary>
        /// Gets the value of the specified field cast to type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The expected type of the field value.</typeparam>
        /// <param name="name">The name of the field to retrieve.</param>
        /// <returns>The field value cast to <typeparamref name="T"/>.</returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown if the field <paramref name="name"/> does not exist in this entry.
        /// </exception>
        /// <exception cref="InvalidCastException">
        /// Thrown if the value cannot be cast to <typeparamref name="T"/>.
        /// </exception>
        public T GetValue<T>(string name)
        {
            if (Values.TryGetValue(name, out var value))
                return (T)value;
            throw GetKeyNotFoundException(name);
        }

        /// <summary>
        /// Gets the raw (untyped) value of the specified field.
        /// </summary>
        /// <param name="name">The name of the field to retrieve.</param>
        /// <returns>The field value as <see cref="object"/>.</returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown if the field <paramref name="name"/> does not exist in this entry.
        /// </exception>
        public object? GetRawValue(string name)
        {
            if (Values.TryGetValue(name, out var value))
                return value;
            throw GetKeyNotFoundException(name);
        }

        /// <summary>
        /// Gets the value of the specified field as a string.
        /// </summary>
        /// <param name="name">The name of the field to retrieve.</param>
        /// <returns>
        /// The string representation of the field value, or <see cref="string.Empty"/> if the value is <see langword="null"/>.
        /// </returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown if the field <paramref name="name"/> does not exist in the format.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the field's declared type is not <see cref="LogFormatPartType.TEXT"/>.
        /// </exception>
        public string GetStringValue(string name)
        {
            EnsurePartType(name, LogFormatPartType.TEXT);
            var value = GetValue<object>(name);
            return value?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Gets the value of the specified field as a 32-bit integer.
        /// </summary>
        /// <param name="name">The name of the field to retrieve.</param>
        /// <returns>The field value as <see cref="int"/>.</returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown if the field <paramref name="name"/> does not exist in the format.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the field's declared type is not <see cref="LogFormatPartType.INTEGER"/>.
        /// </exception>
        /// <exception cref="InvalidCastException">
        /// Thrown if the stored value is not an <see cref="int"/>.
        /// </exception>
        public int GetIntegerValue(string name)
        {
            EnsurePartType(name, LogFormatPartType.INTEGER);
            return GetValue<int>(name);
        }

        /// <summary>
        /// Gets the value of the specified field as a single-precision floating-point number.
        /// </summary>
        /// <param name="name">The name of the field to retrieve.</param>
        /// <returns>The field value as <see cref="float"/>.</returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown if the field <paramref name="name"/> does not exist in the format.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the field's declared type is not <see cref="LogFormatPartType.FLOAT"/>.
        /// </exception>
        /// <exception cref="InvalidCastException">
        /// Thrown if the stored value is not a <see cref="float"/>.
        /// </exception>
        public float GetFloatValue(string name)
        {
            EnsurePartType(name, LogFormatPartType.FLOAT);
            return GetValue<float>(name);
        }

        /// <summary>
        /// Gets the value of the specified field as a double-precision floating-point number.
        /// </summary>
        /// <param name="name">The name of the field to retrieve.</param>
        /// <returns>The field value as <see cref="double"/>.</returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown if the field <paramref name="name"/> does not exist in the format.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the field's declared type is not <see cref="LogFormatPartType.FLOAT"/>.
        /// </exception>
        /// <exception cref="InvalidCastException">
        /// Thrown if the stored value is not a <see cref="double"/>.
        /// </exception>
        public double GetDoubleValue(string name)
        {
            EnsurePartType(name, LogFormatPartType.FLOAT);
            return GetValue<double>(name);
        }

        /// <summary>
        /// Gets the value of the specified field as a date and time.
        /// </summary>
        /// <param name="name">The name of the field to retrieve.</param>
        /// <returns>The field value as <see cref="DateTime"/>.</returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown if the field <paramref name="name"/> does not exist in the format.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the field's declared type is not <see cref="LogFormatPartType.DATETIME"/>.
        /// </exception>
        /// <exception cref="InvalidCastException">
        /// Thrown if the stored value is not a <see cref="DateTime"/>.
        /// </exception>
        public DateTime GetDateTimeValue(string name)
        {
            EnsurePartType(name, LogFormatPartType.DATETIME);
            return GetValue<DateTime>(name);
        }

        /// <summary>
        /// Validates that the specified field exists in the format and has the expected semantic type.
        /// </summary>
        /// <param name="name">The name of the field to validate.</param>
        /// <param name="expectedType">The expected <see cref="LogFormatPartType"/>.</param>
        /// <exception cref="KeyNotFoundException">
        /// Thrown if the field <paramref name="name"/> is not defined in the format.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the field's actual type does not match <paramref name="expectedType"/>.
        /// </exception>
        private void EnsurePartType(string name, LogFormatPartType expectedType)
        {
            if (!Format.PartNames.Contains(name))
                throw new KeyNotFoundException($"Log format does not contain a part named '{name}'.");

            ILogFormatPart part = Format[name];

            if (part.Type != expectedType)
            {
                throw new InvalidOperationException(
                    $"Part '{name}' is of type {part.Type}, but {expectedType} was expected.");
            }
        }

        /// <summary>
        /// Creates a <see cref="KeyNotFoundException"/> for a missing field.
        /// </summary>
        /// <param name="name">The name of the missing field.</param>
        /// <returns>A new <see cref="KeyNotFoundException"/> instance.</returns>
        private static Exception GetKeyNotFoundException(string name)
        {
            return new KeyNotFoundException($"Log entry does not contain a value for part '{name}'.");
        }
    }
}
