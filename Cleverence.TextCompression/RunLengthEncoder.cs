using System.Runtime.CompilerServices;
using System.Text;

namespace Cleverence.TextCompression
{
    /// <summary>
    /// Implements the Run-Length Encoding (RLE) algorithm for string compression and decompression.
    /// This implementation assumes input strings consist only of lowercase English letters ('a'-'z')
    /// for uncompressed data, and lowercase letters plus digits for compressed data.
    /// </summary>
    public sealed class RunLengthEncoder : IStringTransformer
    {
        /// <summary>
        /// Max length for string in .NET
        /// </summary>
        public const int MaxStringLength = 0x3FFFFFDF;

        /// <summary>
        /// Validates that the input string contains only allowed characters based on the compression state.
        /// </summary>
        /// <param name="input">The string to validate.</param>
        /// <param name="isCompressed"><c>true</c> if the input is expected to be compressed (allowing digits and 'a'-'z'); <c>false</c> if uncompressed (allowing only 'a'-'z').</param>
        /// <exception cref="System.ArgumentException">Thrown if the input contains characters outside the allowed set for the specified mode.</exception>
        private void EnsureInputHasOnlyAllowedChars(string input, bool isCompressed)
        {
            if (string.IsNullOrEmpty(input))
            {
                return;
            }

            if (isCompressed)
            {
                for (int i = 0; i < input.Length; i++)
                {
                    char c = input[i];
                    bool isLowerLetter = (c >= 'a' && c <= 'z');
                    bool isDigit = char.IsDigit(c);

                    if (!isLowerLetter && !isDigit)
                    {
                        throw new ArgumentException(
                            $"Compressed input contains an invalid character: '{c}'. " +
                            $"Only lowercase letters (a-z) and digits are allowed.",
                            nameof(input));
                    }
                }
            }
            else
            {
                for (int i = 0; i < input.Length; i++)
                {
                    char c = input[i];
                    if (c < 'a' || c > 'z')
                    {
                        throw new ArgumentException(
                            $"Uncompressed input contains an invalid character: '{c}'. " +
                            $"Only lowercase letters (a-z) are allowed.",
                            nameof(input));
                    }
                }
            }
        }

        /// <summary>
        /// Appends a specified character a given number of times to a <see cref="StringBuilder"/>,
        /// throwing an <see cref="OverflowException"/> if the resulting string length exceeds a predefined maximum <see cref="MaxStringLength"/>.
        /// </summary>
        /// <remarks>
        /// This method is aggressively inlined by the JIT compiler for performance optimization.
        /// It checks the length *before* appending to prevent exceeding the allowed maximum length defined by <see cref="MaxStringLength"/>.
        /// </remarks>
        /// <param name="sb">The <see cref="StringBuilder"/> to append the characters to.</param>
        /// <param name="c">The character to append.</param>
        /// <param name="count">The number of times to append the character. Defaults to 1.</param>
        /// <exception cref="OverflowException">
        /// Thrown if appending <paramref name="count"/> characters to the current length of <paramref name="sb"/>
        /// would result in a length greater than <see cref="MaxStringLength"/>.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AppendWithThrow(StringBuilder sb, char c, int count = 1)
        {
            if (sb.Length + count > MaxStringLength)
            {
                throw new OverflowException(
                            $"Decompressed string exceeds the maximum allowed length of {MaxStringLength}. " +
                            $"Current result length is {sb.Length}, attempting to add additional {count} characters.");
            }
            sb.Append(c, count);
        }

        /// <summary>
        /// Compresses the input string using Run-Length Encoding.
        /// The output format is: character followed by its count (e.g., "aaabbc" becomes "a3b2c1").
        /// </summary>
        /// <param name="input">The uncompressed string, containing only lowercase letters ('a'-'z').</param>
        /// <returns>The compressed string.</returns>
        /// <exception cref="System.ArgumentException">Thrown if the input contains invalid characters.</exception>
        public string Compress(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            EnsureInputHasOnlyAllowedChars(input, false);

            var result = new StringBuilder();
            int i = 0;

            while (i < input.Length)
            {
                char currentChar = input[i];
                int count = 1;
                int j = i + 1;

                while (j < input.Length && input[j] == currentChar)
                {
                    count++;
                    j++;
                }

                result.Append(currentChar);

                if (count > 1)
                {
                    result.Append(count);
                }

                i = j;
            }

            return result.ToString();
        }

        /// <summary>
        /// Decompresses a string previously compressed using RLE.
        /// The format expected is: character followed by a sequence of digits representing the count.
        /// </summary>
        /// <param name="compressedInput">The RLE encoded string.</param>
        /// <returns>The original, decompressed string.</returns>
        /// <exception cref="System.ArgumentException">Thrown if the input contains invalid characters for compressed data.</exception>
        /// <exception cref="System.FormatException">Thrown if a sequence of digits representing the count is malformed or cannot be parsed.</exception>
        public string Decompress(string compressedInput)
        {
            if (string.IsNullOrEmpty(compressedInput))
            {
                return string.Empty;
            }

            EnsureInputHasOnlyAllowedChars(compressedInput, true);

            var result = new StringBuilder();
            int i = 0;

            while (i < compressedInput.Length)
            {
                char currentChar = compressedInput[i];

                i++;

                if (i >= compressedInput.Length)
                {
                    AppendWithThrow(result, currentChar);
                    break;
                }

                var countString = new StringBuilder();

                while (i < compressedInput.Length && char.IsDigit(compressedInput[i]))
                {
                    countString.Append(compressedInput[i]);
                    i++;
                }

                if (countString.Length == 0)
                {
                    AppendWithThrow(result, currentChar);
                    continue;
                }

                if (int.TryParse(countString.ToString(), out int count))
                {
                    if (count == 0) throw new ArgumentException($"Count can`t be zero in compressed data: {countString}");
                    AppendWithThrow(result, currentChar, count);
                }
                else
                {
                    throw new FormatException($"Invalid count format encountered in compressed data: {countString}");
                }
            }

            return result.ToString();
        }
    }
}
