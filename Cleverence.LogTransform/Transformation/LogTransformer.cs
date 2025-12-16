using Cleverence.LogTransform.Formats;
using Cleverence.LogTransform.Models;
using System.Diagnostics.CodeAnalysis;

namespace Cleverence.LogTransform.Transformation
{
    /// <summary>
    /// Transforms log entries using a preconfigured map of format-specific transformers.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class uses a <see cref="LogTransformMap"/> to look up the appropriate transformer based on
    /// the input log entry's format. It provides both safe (<see cref="TryTransform"/>) and throwing
    /// (<see cref="Transform"/>) transformation methods.
    /// </para>
    /// <para>
    /// The class assumes that the transformation map is correctly initialized and that all registered
    /// transformers are compatible with their expected input formats.
    /// </para>
    /// <para>
    /// This class is stateless (except for the immutable transform map) and thread-safe.
    /// </para>
    /// </remarks>
    public sealed class LogTransformer
    {
        private readonly LogTransformMap _transformMap;

        /// <summary>
        /// Initializes a new instance of <see cref="LogTransformer"/> with the specified transformation map.
        /// </summary>
        /// <param name="transformMap">
        /// A preconfigured map of input formats to transformers. Must not be <see langword="null"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="transformMap"/> is <see langword="null"/>.
        /// </exception>
        public LogTransformer(LogTransformMap transformMap)
        {
            ArgumentNullException.ThrowIfNull(transformMap, nameof(transformMap));

            _transformMap = transformMap;
        }

        /// <summary>
        /// Transforms the specified log entry using the registered transformer for its format.
        /// </summary>
        /// <param name="entry">The log entry to transform. Must not be <see langword="null"/>.</param>
        /// <returns>A new log entry in the target format.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="entry"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if the format of <paramref name="entry"/> is not registered in the transformation map.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the transformation fails unexpectedly (e.g., due to a bug in the transformer).
        /// </exception>
        public LogEntry Transform(LogEntry entry)
        {
            ArgumentNullException.ThrowIfNull(entry, nameof(entry));

            if (!_transformMap.InputFormats.Contains(entry.Format))
            {
                throw new ArgumentException(
                    $"Log entry format '{entry.Format}' does not present in transformation map.",
                    nameof(entry));
            }

            if (!TryTransform(entry, out var transformedEntry))
            {
                throw new InvalidOperationException("Transformation failed unexpectedly.");
            }

            return transformedEntry;
        }

        /// <summary>
        /// Attempts to transform the specified log entry using the registered transformer for its format.
        /// </summary>
        /// <param name="entry">The log entry to transform.</param>
        /// <param name="result">
        /// When this method returns, contains the transformed log entry if successful; otherwise, <see langword="null"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the entry's format is supported and the transformation succeeded;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// This method returns <see langword="false"/> in the following cases:
        /// <list type="bullet">
        ///   <item>The input <paramref name="entry"/> is <see langword="null"/>.</item>
        ///   <item>The entry's format is not registered in the transformation map.</item>
        ///   <item>The corresponding transformer throws an exception during transformation.</item>
        /// </list>
        /// Exceptions thrown by the underlying transformer are caught and converted to a <see langword="false"/> result.
        /// </remarks>
        public bool TryTransform(LogEntry entry, [NotNullWhen(true)] out LogEntry? result)
        {
            result = null;

            if (entry is null || !_transformMap.InputFormats.Contains(entry.Format))
                return false;

            if (!_transformMap.TryGetTransformer(entry.Format, out var transformer))
            {
                return false;
            }

            try
            {
                result = transformer.Transform(entry);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
