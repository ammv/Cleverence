using Cleverence.LogTransform.Models;
using System.Diagnostics.CodeAnalysis;

namespace Cleverence.LogTransform.Formatting
{
    /// <summary>
    /// Formats a <see cref="LogEntry"/> into a string according to its log format.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class uses the <see cref="ILogFormatPart.Formatter"/> of each part to convert values to strings.
    /// If any formatter fails (returns <see langword="false"/>), the entire formatting operation fails.
    /// </para>
    /// <para>
    /// The resulting string is constructed by joining formatted parts with the <see cref="ILogFormat.Separator"/>
    /// defined in the entry's format.
    /// </para>
    /// <para>
    /// This class is stateless and thread-safe.
    /// </para>
    /// </remarks>
    public sealed class LogFormatter
    {
        /// <summary>
        /// Attempts to format the specified log entry into a string.
        /// </summary>
        /// <param name="entry">The log entry to format. Must not be <see langword="null"/>.</param>
        /// <param name="result">
        /// When this method returns, contains the formatted string if successful; otherwise, <see langword="null"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if formatting succeeded for all parts; otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="entry"/> is <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// Formatting fails if:
        /// <list type="bullet">
        ///   <item>Any part's <see cref="ILogFormatPart.Formatter"/> returns <see langword="false"/>.</item>
        ///   <item>A formatter throws an exception (caught internally and converted to <see langword="false"/>).</item>
        /// </list>
        /// The order of parts in the output matches <see cref="ILogFormat.PartNames"/>.
        /// </remarks>
        public bool TryFormat(LogEntry entry, [NotNullWhen(true)] out string? result)
        {
            ArgumentNullException.ThrowIfNull(entry, nameof(entry));

            var parts = new string[entry.Format.PartNames.Count];
            for (int i = 0; i < parts.Length; i++)
            {
                var name = entry.Format.PartNames[i];
                var logPart = entry.Format[name];
                var value = entry.Values.TryGetValue(name, out var obj) ? obj : null;

                var tryFormatter = logPart.Formatter;
                if (!tryFormatter.TryInvoke(value, out var formattedPart))
                {
                    result = null;
                    return false;
                }

                parts[i] = formattedPart;
            }

            result = string.Join(entry.Format.Separator, parts);
            return true;
        }

        /// <summary>
        /// Formats the specified log entry into a string.
        /// </summary>
        /// <param name="entry">The log entry to format. Must not be <see langword="null"/>.</param>
        /// <returns>The formatted string.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="entry"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if any part's formatter fails (returns <see langword="false"/> or throws an exception).
        /// </exception>
        public string Format(LogEntry entry)
        {
            if (!TryFormat(entry, out var result))
            {
                throw new InvalidOperationException("Failed to format log entry. One or more formatters threw an exception.");
            }
            return result;
        }
    }
}
