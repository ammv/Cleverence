using Cleverence.LogTransform.Formats;
using Cleverence.LogTransform.Transformation;

namespace Cleverence.LogTransform.Tests.Helpers
{
    internal class FormatBoundLogTransformerFactory
    {
        internal static FormatBoundLogTransformer CreateTransformer(
    ILogFormat outputFormat,
    ILogFormat? expectedInputFormat = null)
        {
            expectedInputFormat ??= new MockLogFormat(new string[0], new Dictionary<string, ILogFormatPart>());
            return FormatBoundLogTransformer.Create(
                expectedInputFormat,
                entry => LogEntryFactory.CreateLogEntry(outputFormat));
        }
    }
}
