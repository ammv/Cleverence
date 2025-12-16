using System.Linq;

namespace Cleverence.LogTransform.Formats
{
    /// <summary>
    /// Provides a base implementation for structured log formats, implementing common equality, validation,
    /// and property storage logic.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This abstract class handles:
    /// <list type="bullet">
    ///   <item><description>Storage and validation of part names and parts dictionary.</description></item>
    ///   <item><description>Indexer access to parts by name.</description></item>
    ///   <item><description>Structural equality and hash code generation based on format definition.</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Derived classes must:
    /// <list type="bullet">
    ///   <item><description>Implement <see cref="TryParse"/> to define format-specific parsing logic.</description></item>
    ///   <item><description>Override <see cref="GetEqualityComponentsInternal"/> to include format-specific equality components (e.g., regex pattern, quoting flag).</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// All instances are immutable and thread-safe.
    /// </para>
    /// </remarks>
    public abstract class LogFormatBase : ILogFormat
    {
        private readonly IReadOnlyDictionary<string, ILogFormatPart> _parts;

        /// <summary>
        /// Gets the character used to separate fields in the log line.
        /// </summary>
        public char Separator { get; }

        /// <summary>
        /// Gets the ordered list of part names defined by this log format.
        /// </summary>
        public IReadOnlyList<string> PartNames { get; }

        /// <summary>
        /// Gets the log format part associated with the specified name.
        /// </summary>
        /// <param name="name">The name of the part to retrieve.</param>
        /// <returns>The <see cref="ILogFormatPart"/> instance for the given name.</returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown if no part with the specified <paramref name="name"/> exists.
        /// </exception>
        public ILogFormatPart this[string name] => _parts[name];

        /// <summary>
        /// Initializes a new instance with the specified separator and part set.
        /// </summary>
        /// <param name="separator">The field separator character.</param>
        /// <param name="partSet">An immutable set of named log format parts. Must not be <see langword="null"/>.</param>
        public LogFormatBase(char separator, LogFormatPartSet partSet) : this(separator, partSet.PartNames, partSet.Parts)
        {
        }

        /// <summary>
        /// Initializes a new instance with the specified separator, part names, and parts dictionary.
        /// </summary>
        /// <param name="separator">The field separator character.</param>
        /// <param name="partNames">The ordered list of part names. Must not be <see langword="null"/>.</param>
        /// <param name="parts">A dictionary mapping part names to their definitions. Must not be <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="partNames"/> or <paramref name="parts"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if:
        /// <list type="bullet">
        ///   <item>The <paramref name="parts"/> dictionary is empty.</item>
        ///   <item>The number of <paramref name="partNames"/> does not match the number of <paramref name="parts"/>.</item>
        ///   <item>Any name in <paramref name="partNames"/> is <see langword="null"/> or missing from <paramref name="parts"/>.</item>
        /// </list>
        /// </exception>
        public LogFormatBase(char separator, IEnumerable<string> partNames, IReadOnlyDictionary<string, ILogFormatPart> parts)
        {
            ArgumentNullException.ThrowIfNull(partNames, nameof(partNames));
            ArgumentNullException.ThrowIfNull(parts, nameof(parts)); // Note: corrected from original

            var partNamesArray = partNames.ToArray();

            if (parts.Count == 0)
            {
                throw new ArgumentException("The parts dictionary must contain at least one entry.", nameof(parts));
            }

            if (partNamesArray.Length != parts.Count)
            {
                throw new ArgumentException(
                    $"The number of part names ({partNamesArray.Length}) does not match the number of parts ({parts.Count}).",
                    nameof(partNames));
            }

            foreach (var name in partNamesArray)
            {
                if (name is null)
                {
                    throw new ArgumentException("Part name cannot be null.", nameof(partNames));
                }

                if (!parts.ContainsKey(name))
                {
                    throw new ArgumentException(
                        $"Part name '{name}' is listed in partNames but is not present in the parts dictionary.",
                        nameof(partNames));
                }
            }

            Separator = separator;
            PartNames = partNamesArray;
            _parts = parts;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is not LogFormatBase other || GetType() != other.GetType())
                return false;

            return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hash = new HashCode();
            foreach (var component in GetEqualityComponents())
                hash.Add(component);
            return hash.ToHashCode();
        }

        /// <summary>
        /// Attempts to parse a raw log line into structured values according to the specific format.
        /// </summary>
        /// <param name="log">The raw log line to parse.</param>
        /// <param name="values">The resulting dictionary of parsed values, or <see langword="null"/> if parsing failed.</param>
        /// <returns>
        /// <see langword="true"/> if parsing succeeded; otherwise, <see langword="false"/>.
        /// </returns>
        public abstract bool TryParse(string log, out IReadOnlyDictionary<string, object?> values);

        /// <summary>
        /// Returns the sequence of objects that determine equality for this log format.
        /// Includes common components like separator, part names, and part types.
        /// </summary>
        /// <returns>An enumerable of equality components.</returns>
        protected IEnumerable<object> GetEqualityComponents()
        {
            yield return Separator;
            foreach (var partName in PartNames)
            {
                yield return partName;
            }
            foreach (var kvp in _parts)
            {
                yield return kvp.Value.Type;
            }
            foreach (var component in GetEqualityComponentsInternal())
            {
                yield return component;
            }
        }

        /// <summary>
        /// Returns format-specific equality components that should be included in <see cref="Equals"/> and <see cref="GetHashCode"/>.
        /// </summary>
        /// <returns>An enumerable of additional equality components unique to the derived format.</returns>
        /// <example>
        /// For <see cref="DelimetedLogFormat"/>, this might return the <c>supportQuotedFields</c> flag.
        /// For <see cref="RegexLogFormat"/>, this might return the regex pattern.
        /// </example>
        protected abstract IEnumerable<object> GetEqualityComponentsInternal();
    }
}
