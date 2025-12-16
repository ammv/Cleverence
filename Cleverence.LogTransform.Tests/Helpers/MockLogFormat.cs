using Cleverence.LogTransform.Formats;

namespace Cleverence.LogTransform.Tests.Helpers
{
    internal delegate TResult FuncWithOut<TInput, TOut, TResult>(TInput input, out TOut output);
    internal class MockLogFormat : ILogFormat
    {
        private readonly IReadOnlyDictionary<string, ILogFormatPart> _parts;

        public IReadOnlyList<string> PartNames { get; }

        public char Separator { get; } = default;

        public FuncWithOut<string, IReadOnlyDictionary<string, object?>, bool> TryParseFuncOverride { get; set; }

        public MockLogFormat(IReadOnlyList<string> partNames, IReadOnlyDictionary<string, ILogFormatPart> parts)
        {
            PartNames = partNames;
            _parts = parts;
        }

        public MockLogFormat(
            char separator,
            IReadOnlyList<string> partNames,
            IReadOnlyDictionary<string, ILogFormatPart> parts) : this(partNames, parts)
        {
            PartNames = partNames;
            _parts = parts;
            Separator = separator;
        }

        public ILogFormatPart this[string name] => _parts[name];

        public bool TryParse(string log, out IReadOnlyDictionary<string, object?> values)
        {
            values = default;

            if (TryParseFuncOverride != null)
            {
                return TryParseFuncOverride(log, out values);
            }
            
            return true;
        }
    }
}
