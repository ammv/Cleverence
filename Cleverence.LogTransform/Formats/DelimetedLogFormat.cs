using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Cleverence.LogTransform.Formats
{
    /// <summary>
    /// Represents a log format where fields are separated by a fixed character (e.g., comma or space).
    /// Supports optional handling of double-quoted fields to preserve separators within values.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When <c>supportQuotedFields</c> is <see langword="true"/>, the parser treats content inside double quotes (<c>"</c>)
    /// as a single field, even if it contains the separator character. Only double quotes are recognized;
    /// single quotes and escaped quotes are treated as literal characters.
    /// </para>
    /// <para>
    /// When <c>supportQuotedFields</c> is <see langword="false"/>, the input line is split strictly on the separator,
    /// but only into as many fields as defined by <see cref="ILogFormat.PartNames.Count"/>. Any remaining separator
    /// characters in the tail of the line become part of the last field.
    /// </para>
    /// <para>
    /// This class is immutable and thread-safe.
    /// </para>
    /// </remarks>
    public sealed class DelimetedLogFormat : LogFormatBase
    {
        private readonly bool _supportQuotedFields;

        /// <summary>
        /// Initializes a new instance of <see cref="DelimetedLogFormat"/> with the specified separator, part set, and quoting behavior.
        /// </summary>
        /// <param name="separator">The character used to separate fields in the log line.</param>
        /// <param name="partSet">An immutable set of named log format parts. Must not be <see langword="null"/>.</param>
        /// <param name="supportQuotedFields">
        /// <see langword="true"/> to enable parsing of double-quoted fields; otherwise, <see langword="false"/>.
        /// </param>
        public DelimetedLogFormat(
            char separator,
            LogFormatPartSet partSet,
            bool supportQuotedFields) :
            base(separator, partSet)
        {
            _supportQuotedFields = supportQuotedFields;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="DelimetedLogFormat"/> with the specified separator, part names, parts dictionary, and quoting behavior.
        /// </summary>
        /// <param name="separator">The character used to separate fields in the log line.</param>
        /// <param name="partNames">The ordered list of part names. Must not be <see langword="null"/>.</param>
        /// <param name="parts">A dictionary mapping part names to their definitions. Must not be <see langword="null"/>.</param>
        /// <param name="supportQuotedFields">
        /// <see langword="true"/> to enable parsing of double-quoted fields; otherwise, <see langword="false"/>.
        /// Defaults to <see langword="false"/>.
        /// </param>
        public DelimetedLogFormat(
            char separator,
            IEnumerable<string> partNames,
            IReadOnlyDictionary<string, ILogFormatPart> parts,
            bool supportQuotedFields = false) :
            base(separator, partNames, parts)
        {
            _supportQuotedFields = supportQuotedFields;
        }

        /// <summary>
        /// Attempts to parse a log line into structured values according to the defined format.
        /// </summary>
        /// <param name="log">The raw log line to parse. Can be <see langword="null"/> or empty.</param>
        /// <param name="values">
        /// When this method returns, contains a dictionary of parsed values if parsing succeeded;
        /// otherwise, <see langword="null"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the log line was successfully parsed and all field parsers succeeded;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// Parsing fails if:
        /// <list type="bullet">
        ///   <item><description>The input is <see langword="null"/> or empty.</description></item>
        ///   <item><description>The number of parsed fields does not match <see cref="ILogFormat.PartNames.Count"/>.</description></item>
        ///   <item><description>Any field's parser returns <see langword="false"/>.</description></item>
        /// </list>
        /// </remarks>
        public override bool TryParse(string log, [NotNullWhen(true)] out IReadOnlyDictionary<string, object?> values)
        {
            values = null!;

            if (string.IsNullOrEmpty(log))
                return false;

            string[]? fields = _supportQuotedFields
                ? ParseQuotedFields(log, Separator)
                : log.Split(Separator, PartNames.Count);

            if (fields?.Length != PartNames.Count)
                return false;

            var result = new Dictionary<string, object?>(PartNames.Count);
            for (int i = 0; i < fields.Length; i++)
            {
                var name = PartNames[i];
                var part = this[name];

                if (!part.Parser.TryInvoke(fields[i], out object? parsedValue))
                {
                    return false;
                }
                result.Add(name, parsedValue);
            }

            values = result;
            return true;
        }

        /// <summary>
        /// Splits a log line into fields while respecting double-quoted sections.
        /// </summary>
        /// <param name="line">The log line to split.</param>
        /// <param name="separator">The field separator character.</param>
        /// <returns>An array of field strings, with quotes removed and separators inside quotes preserved.</returns>
        /// <remarks>
        /// This method does not support escaped quotes (e.g., <c>""</c> is not treated as a literal quote).
        /// Unbalanced quotes are accepted, and parsing continues as if quotes were balanced.
        /// </remarks>
        private static string[] ParseQuotedFields(string line, char separator)
        {
            var fields = new List<string>();
            var current = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"' && !inQuotes)
                {
                    inQuotes = true;
                }
                else if (c == '"' && inQuotes)
                {
                    inQuotes = false;
                }
                else if (c == separator && !inQuotes)
                {
                    fields.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            fields.Add(current.ToString());
            return fields.ToArray();
        }

        /// <inheritdoc />
        protected override IEnumerable<object> GetEqualityComponentsInternal()
        {
            yield return _supportQuotedFields;
        }
    }
}
