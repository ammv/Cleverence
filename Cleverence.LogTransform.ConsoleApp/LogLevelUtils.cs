using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Cleverence.LogTransform.ConsoleApp
{
    /// <summary>
    /// Provides utility methods for converting between <see cref="LogLevel"/> enum values and their string representations.
    /// Supports both short (e.g., "INFO") and long (e.g., "INFORMATION") variants for compatible levels.
    /// </summary>
    public static class LogLevelUtils
    {
        private const string _infoShort = "INFO";
        private const string _infoLong = "INFORMATION";
        private const string _debug = "DEBUG";
        private const string _warnShort = "WARN";
        private const string _warnLong = "WARNING";
        private const string _error = "ERROR";

        /// <summary>
        /// Converts a string representation of a log level to its corresponding <see cref="LogLevel"/> enum value.
        /// </summary>
        /// <param name="level">The log level string to convert. Case-insensitive. Must not be null or empty.</param>
        /// <returns>The corresponding <see cref="LogLevel"/> value.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="level"/> is null, empty, or does not match any supported log level.
        /// Supported values: "INFO", "INFORMATION", "DEBUG", "WARN", "WARNING", "ERROR".
        /// </exception>
        public static LogLevel StringToEnum(string level)
        {
            ArgumentException.ThrowIfNullOrEmpty(level, nameof(level));

            level = level.Trim().ToUpperInvariant();

            return level switch
            {
                _infoShort or _infoLong => LogLevel.INFO,
                _debug => LogLevel.DEBUG,
                _warnShort or _warnLong => LogLevel.WARN,
                _error => LogLevel.ERROR,
                _ => throw new ArgumentException($"Invalid log level: \"{level}\"", nameof(level))
            };
        }

        /// <summary>
        /// Converts a <see cref="LogLevel"/> enum value to its string representation.
        /// </summary>
        /// <param name="level">The log level to convert.</param>
        /// <param name="longVariant">
        /// If <see langword="true"/>, uses long forms for INFO ("INFORMATION") and WARN ("WARNING");
        /// otherwise, uses short forms ("INFO", "WARN").
        /// </param>
        /// <returns>The string representation of the log level.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="level"/> is not a supported value.
        /// </exception>
        public static string EnumToString(LogLevel level, bool longVariant = false)
        {
            return level switch
            {
                LogLevel.INFO => longVariant ? _infoLong : _infoShort,
                LogLevel.WARN => longVariant ? _warnLong : _warnShort,
                LogLevel.DEBUG => _debug,
                LogLevel.ERROR => "ERROR",
                _ => throw new ArgumentException($"Unsupported level: \"{level}\"", nameof(level))
            };
        }
    }
}
