using Cleverence.LogTransform.Formats;
using Cleverence.LogTransform.Transformation;
using System.Collections.ObjectModel;

namespace Cleverence.LogTransform.Tests.Helpers
{
    internal class LogTransformMapFactory
    {
        internal static LogTransformMap CreateTransformMap()
        {
            var inputFormat = new MockLogFormat(new string[0], new Dictionary<string, ILogFormatPart>());
            var outputFormat = new MockLogFormat(new string[0], new Dictionary<string, ILogFormatPart>());
            var transformer = FormatBoundLogTransformerFactory.CreateTransformer(outputFormat, inputFormat);
            return new LogTransformMap(
                outputFormat,
                new ReadOnlyDictionary<ILogFormat, FormatBoundLogTransformer>(
                    new Dictionary<ILogFormat, FormatBoundLogTransformer> { [inputFormat] = transformer }));
        }

        internal static LogTransformMap CreateTransformMapWithInput(ILogFormat inputFormat, ILogFormat? outputFormat = null)
        {
            outputFormat ??= new MockLogFormat(new string[0], new Dictionary<string, ILogFormatPart>());
            var transformer = FormatBoundLogTransformerFactory.CreateTransformer(outputFormat, inputFormat);
            return new LogTransformMap(
                outputFormat,
                new ReadOnlyDictionary<ILogFormat, FormatBoundLogTransformer>(
                    new Dictionary<ILogFormat, FormatBoundLogTransformer> { [inputFormat] = transformer }));
        }
    }
}
