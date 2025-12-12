namespace Cleverence.TextCompression
{
    /// <summary>
    /// Defines a contract for classes that implement string transformation algorithms,
    /// such as compression and decompression.
    /// </summary>
    public interface IStringTransformer
    {
        /// <summary>
        /// Compresses the input string using the specific transformation algorithm.
        /// </summary>
        /// <param name="input">The original string to be compressed.</param>
        /// <returns>The compressed (transformed) version of the string.</returns>
        string Compress(string input);

        /// <summary>
        /// Decompresses the input data using the specific transformation algorithm.
        /// </summary>
        /// <param name="compressedData">The string containing the compressed data.</param>
        /// <returns>The original (decompressed) string.</returns>
        string Decompress(string compressedData);
    }
}
