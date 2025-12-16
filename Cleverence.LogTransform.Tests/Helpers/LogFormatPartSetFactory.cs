using Cleverence.LogTransform.Formats;

namespace Cleverence.LogTransform.Tests.Helpers
{
    internal class LogFormatPartSetFactory
    {
        internal static LogFormatPartSet CreatePartSet(string[] strings, Dictionary<string, ILogFormatPart> dictionary)
        {
            return new LogFormatPartSet(strings, dictionary);
        }
    }
}