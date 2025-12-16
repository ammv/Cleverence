namespace Cleverence.LogTransform.Formats
{
    /// <summary>
    /// Represents an immutable set of named log format parts, preserving the order of part names.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This struct ensures that:
    /// <list type="bullet">
    ///   <item><description>The number of part names matches the number of parts in the dictionary.</description></item>
    ///   <item><description>Every name in <see cref="PartNames"/> exists as a key in <see cref="Parts"/>.</description></item>
    ///   <item><description>The order of <see cref="PartNames"/> is preserved and can be used to reconstruct log lines.</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Instances are immutable and safe for concurrent use. The struct is allocated on the stack (or inline) due to its <c>readonly struct</c> declaration.
    /// </para>
    /// </remarks>
    public readonly struct LogFormatPartSet
    {
        /// <summary>
        /// Gets the ordered list of part names. The order reflects the sequence of fields in the source log format.
        /// </summary>
        /// <remarks>
        /// This list is read-only and must not be modified by the caller.
        /// </remarks>
        public IReadOnlyList<string> PartNames { get; }

        /// <summary>
        /// Gets the dictionary mapping part names to their definitions.
        /// </summary>
        /// <remarks>
        /// This dictionary is read-only and must not be modified by the caller.
        /// </remarks>
        public IReadOnlyDictionary<string, ILogFormatPart> Parts { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="LogFormatPartSet"/>.
        /// </summary>
        /// <param name="partNames">
        /// An ordered list of unique part names. Must not be <see langword="null"/> and must have the same count as <paramref name="parts"/>.
        /// </param>
        /// <param name="parts">
        /// A dictionary of part definitions keyed by name. Must not be <see langword="null"/> and must contain an entry for every name in <paramref name="partNames"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if either <paramref name="partNames"/> or <paramref name="parts"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if:
        /// <list type="bullet">
        ///   <item>The count of <paramref name="partNames"/> does not equal the count of <paramref name="parts"/>.</item>
        ///   <item>Any name in <paramref name="partNames"/> is not present as a key in <paramref name="parts"/>.</item>
        /// </list>
        /// </exception>
        public LogFormatPartSet(
            IReadOnlyList<string> partNames,
            IReadOnlyDictionary<string, ILogFormatPart> parts)
        {
            PartNames = partNames ?? throw new ArgumentNullException(nameof(partNames));
            Parts = parts ?? throw new ArgumentNullException(nameof(parts));

            if (partNames.Count != parts.Count)
                throw new ArgumentException("PartNames and Parts must have the same number of elements.");

            foreach (var name in partNames)
            {
                if (!parts.ContainsKey(name))
                    throw new ArgumentException($"Part name '{name}' is listed in PartNames but not in Parts.");
            }
        }
    }
}
