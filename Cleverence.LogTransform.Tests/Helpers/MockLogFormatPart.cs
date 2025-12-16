using Cleverence.LogTransform.Formats;
using Cleverence.LogTransform.Models;
using Cleverence.LogTransform.Utils;

namespace Cleverence.LogTransform.Tests.Helpers
{
    public class MockLogFormatPart : ILogFormatPart
    {
        public LogFormatPartType TypeOverride { get; init; } = LogFormatPartType.TEXT;
        public LogFormatPartType Type => TypeOverride;
        public TryFunc<string, object?> Parser { get; } = new TryFunc<string, object?>((s) => s);
        public TryFunc<object?, string> Formatter { get; } = new TryFunc<object?, string>(o => o?.ToString() ?? string.Empty);

        public MockLogFormatPart()
        {

        }

        public MockLogFormatPart(Func<string, object?> parser = null, Func<object?, string> formatter = null)
        {
            if (parser != null)
            {
                Parser = new TryFunc<string, object?>(parser);
            }
            if (formatter != null)
            {
                Formatter = new TryFunc<object?, string>(formatter);
            }
        }
    }
}
