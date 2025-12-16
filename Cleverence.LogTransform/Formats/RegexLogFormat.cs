using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Cleverence.LogTransform.Formats
{
    /// <summary>
    /// Represents a log format defined by a regular expression with named capture groups.
    /// Each named group in the regex corresponds to a log format part.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class maps regex named groups to structured log fields:
    /// <list type="bullet">
    ///   <item><description>The regex must contain exactly one named group for each part defined in <see cref="ILogFormat.PartNames"/>.</description></item>
    ///   <item><description>Group names must match part names exactly (case-sensitive).</description></item>
    ///   <item><description>Non-matching groups are not allowed; every part must have a corresponding named group.</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// During parsing:
    /// <list type="bullet">
    ///   <item><description>If a group does not participate in the match (<c>group.Success == false</c>), the parser is invoked with <see langword="null"/>.</description></item>
    ///   <item><description>If parsing fails for any field (including null inputs), the entire parse operation fails.</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// This class is immutable and thread-safe.
    /// </para>
    /// </remarks>
    public sealed class RegexLogFormat : LogFormatBase
    {
        private readonly Regex _regex;

        /// <summary>
        /// Initializes a new instance of <see cref="RegexLogFormat"/> with the specified separator, regex pattern, part set, and regex options.
        /// </summary>
        /// <param name="separator">The character used to separate fields when formatting log entries (not used for parsing).</param>
        /// <param name="pattern">The regular expression pattern with named capture groups. Must not be <see langword="null"/>.</param>
        /// <param name="partSet">An immutable set of log format parts. Must not be <see langword="null"/>.</param>
        /// <param name="options">
        /// Regex compilation options. Defaults to <see cref="RegexOptions.Compiled"/> | <see cref="RegexOptions.ExplicitCapture"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="pattern"/> or <paramref name="partSet"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if:
        /// <list type="bullet">
        ///   <item>The number of named regex groups does not match the number of parts in <paramref name="partSet"/>.</item>
        ///   <item>Any part name in <paramref name="partSet"/> does not correspond to a named group in the regex pattern.</item>
        /// </list>
        /// </exception>
        public RegexLogFormat(
            char separator,
            string pattern,
            LogFormatPartSet partSet,
            RegexOptions options = RegexOptions.Compiled | RegexOptions.ExplicitCapture)
            : base(separator, partSet)
        {
            ArgumentNullException.ThrowIfNull(pattern, nameof(pattern));

            _regex = new Regex(pattern, options);

            var groupNames = _regex.GetGroupNames().Where(n => n != "0").ToHashSet();

            if (groupNames.Count != PartNames.Count)
            {
                throw new ArgumentException(
                    $"The number of regex pattern groups ({groupNames.Count}) does not match the number of parts ({PartNames.Count}).",
                    nameof(pattern));
            }

            foreach (var name in PartNames)
            {
                if (!groupNames.Contains(name))
                    throw new ArgumentException($"Regex pattern does not contain named group '{name}'.", nameof(pattern));
            }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="RegexLogFormat"/> with the specified separator, regex pattern, part names, parts dictionary, and regex options.
        /// </summary>
        /// <param name="separator">The character used to separate fields when formatting log entries (not used for parsing).</param>
        /// <param name="pattern">The regular expression pattern with named capture groups. Must not be <see langword="null"/>.</param>
        /// <param name="partNames">The ordered list of part names. Must not be <see langword="null"/>.</param>
        /// <param name="parts">A dictionary mapping part names to their definitions. Must not be <see langword="null"/>.</param>
        /// <param name="options">
        /// Regex compilation options. Defaults to <see cref="RegexOptions.Compiled"/> | <see cref="RegexOptions.ExplicitCapture"/>.
        /// </param>
        public RegexLogFormat(
            char separator,
            string pattern,
            IEnumerable<string> partNames,
            IReadOnlyDictionary<string, ILogFormatPart> parts,
            RegexOptions options = RegexOptions.Compiled | RegexOptions.ExplicitCapture)
            : this(separator, pattern, new LogFormatPartSet(partNames.ToArray(), parts), options)
        {
        }

        /// <inheritdoc />
        public override bool TryParse(
            string log,
            [NotNullWhen(true)] out IReadOnlyDictionary<string, object?>? values)
        {
            values = null;

            if (string.IsNullOrEmpty(log))
                return false;

            var match = _regex.Match(log);
            if (!match.Success)
                return false;

            var result = new Dictionary<string, object?>(PartNames.Count);
            foreach (var name in PartNames)
            {
                var part = this[name];
                var group = match.Groups[name];

                if (!group.Success)
                {
                    if (part.Parser.TryInvoke(null, out var value))
                    {
                        result.Add(name, value);
                        continue;
                    }
                    return false;
                }
                else
                {
                    if (part.Parser.TryInvoke(group.Value, out var value))
                    {
                        result.Add(name, value);
                        continue;
                    }
                    return false;
                }
            }

            values = result;
            return true;
        }

        /// <inheritdoc />
        protected override IEnumerable<object> GetEqualityComponentsInternal()
        {
            yield return _regex.ToString();
        }
    }
}
