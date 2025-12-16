using Cleverence.LogTransform.EqualityComparers;
using Cleverence.LogTransform.Formats;
using System.Collections.ObjectModel;

namespace Cleverence.LogTransform.Transformation
{
    /// <summary>
    /// Provides a fluent builder for constructing immutable <see cref="LogTransformMap"/> instances.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This builder allows registration of multiple input-to-output transformations that all produce
    /// log entries in the same unified <see cref="LogTransformMap.OutputFormat"/>.
    /// </para>
    /// <para>
    /// It enforces:
    /// <list type="bullet">
    ///   <item>That each input format is registered at most once.</item>
    ///   <item>That at least one transformation is added before building.</item>
    /// </list>
    /// </para>
    /// <para>
    /// The builder uses <see cref="LogFormatEqualityComparer"/> to correctly compare log formats as dictionary keys.
    /// </para>
    /// </remarks>
    public sealed class LogTransformMapBuilder
    {
        private readonly ILogFormat _outputFormat;
        private readonly Dictionary<ILogFormat, FormatBoundLogTransformer> _transformers = new(LogFormatEqualityComparer.Instance);

        /// <summary>
        /// Initializes a new instance of <see cref="LogTransformMapBuilder"/> with the specified output format.
        /// </summary>
        /// <param name="outputFormat">
        /// The unified output log format that all registered transformers must produce.
        /// Must not be <see langword="null"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="outputFormat"/> is <see langword="null"/>.
        /// </exception>
        public LogTransformMapBuilder(ILogFormat outputFormat)
        {
            ArgumentNullException.ThrowIfNull(outputFormat, nameof(outputFormat));
            _outputFormat = outputFormat;
        }

        /// <summary>
        /// Registers a transformer for a specific input log format.
        /// </summary>
        /// <param name="inputFormat">
        /// The input log format to associate with the transformer. Must not be <see langword="null"/>.
        /// </param>
        /// <param name="transformer">
        /// The transformer that converts from <paramref name="inputFormat"/> to the builder's output format.
        /// Must not be <see langword="null"/>.
        /// </param>
        /// <returns>The same builder instance to allow method chaining.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if either <paramref name="inputFormat"/> or <paramref name="transformer"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if a transformer for <paramref name="inputFormat"/> has already been registered.
        /// </exception>
        public LogTransformMapBuilder Add(
            ILogFormat inputFormat,
            FormatBoundLogTransformer transformer)
        {
            ArgumentNullException.ThrowIfNull(inputFormat, nameof(inputFormat));
            ArgumentNullException.ThrowIfNull(transformer, nameof(transformer));

            if (_transformers.ContainsKey(inputFormat))
            {
                throw new InvalidOperationException(
                    $"A transformation for input format '{inputFormat}' is already registered.");
            }

            _transformers.Add(inputFormat, transformer);
            return this;
        }

        /// <summary>
        /// Builds and returns an immutable <see cref="LogTransformMap"/> from the registered transformers.
        /// </summary>
        /// <returns>A new <see cref="LogTransformMap"/> instance.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if no transformations have been registered.
        /// </exception>
        public LogTransformMap Build()
        {
            if (_transformers.Count == 0)
            {
                throw new InvalidOperationException(
                    "At least one transformation must be added before building the map.");
            }

            var readOnlyTransformers = new ReadOnlyDictionary<ILogFormat, FormatBoundLogTransformer>(_transformers);
            return new LogTransformMap(_outputFormat, readOnlyTransformers);
        }
    }
}
