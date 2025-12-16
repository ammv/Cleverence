using Cleverence.LogTransform.Formats;
using Cleverence.LogTransform.Models;
using System.Collections.ObjectModel;

namespace Cleverence.LogTransform.Tests.Helpers
{
    internal class LogEntryFactory
    {
        internal static LogEntry CreateLogEntry(ILogFormat format)
        {
            var values = new ReadOnlyDictionary<string, object?>(
                new Dictionary<string, object?>());
            return new LogEntry(format, values);
        }
        internal static LogEntry CreateLogEntry(ILogFormat format, IReadOnlyDictionary<string, object?> values)
        {
            return new LogEntry(format, values);
        }
    }
}
