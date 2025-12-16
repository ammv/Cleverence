using Cleverence.LogTransform.Formats;

namespace Cleverence.LogTransform.EqualityComparers
{
    /// <summary>
    /// Provides structural equality comparison for <see cref="ILogFormat"/> instances.
    /// Two log formats are considered equal if they have:
    /// <list type="bullet">
    ///   <item><description>The same separator character.</description></item>
    ///   <item><description>The same number and order of part names.</description></item>
    ///   <item><description>Matching part types for each named part.</description></item>
    /// </list>
    /// This comparer is used internally by collections such as <see cref="Dictionary{TKey, TValue}"/>
    /// to ensure correct behavior when log formats are used as keys.
    /// </summary>
    /// <remarks>
    /// This class implements the singleton pattern. Use <see cref="Instance"/> to obtain the default comparer.
    /// </remarks>
    public sealed class LogFormatEqualityComparer : IEqualityComparer<ILogFormat>
    {
        /// <summary>
        /// Gets the singleton instance of <see cref="LogFormatEqualityComparer"/>.
        /// </summary>
        public static readonly LogFormatEqualityComparer Instance = new();

        private LogFormatEqualityComparer() { }

        /// <summary>
        /// Determines whether two <see cref="ILogFormat"/> objects are equal based on their structure.
        /// </summary>
        /// <param name="x">The first <see cref="ILogFormat"/> to compare.</param>
        /// <param name="y">The second <see cref="ILogFormat"/> to compare.</param>
        /// <returns>
        /// <see langword="true"/> if both objects are <see langword="null"/>, or if they have the same separator,
        /// identical part names in the same order, and matching part types for each name; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Equals(ILogFormat? x, ILogFormat? y)
        {
            if (x is null || y is null) return ReferenceEquals(x, y);
            if (ReferenceEquals(x, y)) return true;
            if (x.Separator != y.Separator) return false;
            if (x.PartNames.Count != y.PartNames.Count) return false;

            for (int i = 0; i < x.PartNames.Count; i++)
                if (x.PartNames[i] != y.PartNames[i])
                    return false;

            foreach (var name in x.PartNames)
            {
                if (x[name].Type != y[name].Type)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns a hash code for the specified <see cref="ILogFormat"/> based on its structural properties.
        /// </summary>
        /// <param name="obj">The <see cref="ILogFormat"/> to hash.</param>
        /// <returns>
        /// A hash code that combines the separator, part names (in order), and part types.
        /// Returns 0 if <paramref name="obj"/> is <see langword="null"/>.
        /// </returns>
        public int GetHashCode(ILogFormat obj)
        {
            if (obj is null) return 0;
            var hash = new HashCode();
            hash.Add(obj.Separator);
            foreach (var name in obj.PartNames)
            {
                hash.Add(name);
                hash.Add(obj[name].Type);
            }
            return hash.ToHashCode();
        }
    }
}
