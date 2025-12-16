using Cleverence.LogTransform.Formats;

namespace Cleverence.LogTransform.Transformation
{
    /// <summary>
    /// Represents an immutable mapping from input log formats to their corresponding transformers,
    /// all producing entries in a single, unified output format.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class acts as a registry that associates each supported input format with a
    /// <see cref="FormatBoundLogTransformer"/> capable of converting it to the common <see cref="OutputFormat"/>.
    /// </para>
    /// <para>
    /// It is typically built using <see cref="LogTransformMapBuilder"/> and is designed to be
    /// shared across multiple components (e.g., <see cref="LogTransformer"/>).
    /// </para>
    /// <para>
    /// This class is immutable and thread-safe.
    /// </para>
    /// </remarks>
    public sealed class LogTransformMap
    {
        private readonly IReadOnlyDictionary<ILogFormat, FormatBoundLogTransformer> _transformers;

        /// <summary>
        /// Gets the unified output log format that all transformers in this map produce.
        /// </summary>
        public ILogFormat OutputFormat { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="LogTransformMap"/>.
        /// </summary>
        /// <param name="outputFormat">
        /// The common output format for all transformations. Must not be <see langword="null"/>.
        /// </param>
        /// <param name="transformers">
        /// A non-empty dictionary mapping input formats to their transformers. Must not be <see langword="null"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if either <paramref name="outputFormat"/> or <paramref name="transformers"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="transformers"/> is empty.
        /// </exception>
        internal LogTransformMap(
            ILogFormat outputFormat,
            IReadOnlyDictionary<ILogFormat, FormatBoundLogTransformer> transformers)
        {
            ArgumentNullException.ThrowIfNull(outputFormat, nameof(outputFormat));
            ArgumentNullException.ThrowIfNull(transformers, nameof(transformers));

            if (transformers.Count == 0)
                throw new ArgumentException("At least one transformation must be registered.", nameof(transformers));

            OutputFormat = outputFormat;
            _transformers = transformers;
        }

        /// <summary>
        /// Attempts to get the transformer associated with the specified input format.
        /// </summary>
        /// <param name="inputFormat">The input log format to look up.</param>
        /// <param name="transformer">
        /// When this method returns, contains the transformer if found; otherwise, <see langword="null"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if a transformer is registered for <paramref name="inputFormat"/>;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public bool TryGetTransformer(ILogFormat inputFormat, out FormatBoundLogTransformer? transformer)
        {
            return _transformers.TryGetValue(inputFormat, out transformer);
        }

        /// <summary>
        /// Gets the transformer associated with the specified input format.
        /// </summary>
        /// <param name="inputFormat">The input log format to look up.</param>
        /// <returns>The registered transformer for the given format.</returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown if no transformer is registered for <paramref name="inputFormat"/>.
        /// </exception>
        public FormatBoundLogTransformer GetTransformer(ILogFormat inputFormat)
        {
            if (!TryGetTransformer(inputFormat, out var transformer))
                throw new KeyNotFoundException($"No transformation registered for the specified input log format.");
            return transformer;
        }

        /// <summary>
        /// Gets the collection of all input log formats supported by this transformation map.
        /// </summary>
        /// <returns>An enumerable of input formats.</returns>
        /// <remarks>
        /// The returned collection is a live view of the internal keys and must not be modified.
        /// </remarks>
        public IEnumerable<ILogFormat> InputFormats => _transformers.Keys;
    }
}
