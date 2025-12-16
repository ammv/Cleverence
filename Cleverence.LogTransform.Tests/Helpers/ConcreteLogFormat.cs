using Cleverence.LogTransform.Formats;
using Cleverence.LogTransform.Formats.Builders;

namespace Cleverence.LogTransform.Tests.Helpers
{
    internal sealed class ConcreteLogFormat : LogFormatBase
    {
        public static ConcreteLogFormat CreateWithFormatId(string formatId)
        {
            var partSet = new LogFormatPartSetBuilder()
                .AddPart(
                    name: "date",
                    type: Models.LogFormatPartType.DATETIME,
                    parser: (x) => DateTime.Parse(x))
                .AddPart(
                    name: "level",
                    type: Models.LogFormatPartType.TEXT,
                    parser: (x) => "debug")
                .AddPart(
                    name: "message",
                    type: Models.LogFormatPartType.TEXT,
                    parser: (x) => "debug")
                .Build();

            return new ConcreteLogFormat(formatId, '\t', partSet);
        }
        public string FormatId { get; }

        public ConcreteLogFormat(string formatId, char separator, LogFormatPartSet partSet) :
            base(separator, partSet)
        {
            FormatId = formatId;
        }

        public ConcreteLogFormat(string formatId, char separator, IEnumerable<string> partNames, IReadOnlyDictionary<string, ILogFormatPart> parts) :
            base(separator, partNames, parts)
        {
            FormatId = formatId;
        }

        public override bool TryParse(string log, out IReadOnlyDictionary<string, object?> values)
        {
            values = new Dictionary<string, object?>()
            {
                {"date", DateTime.Now },
                {"level", "DEBUG" },
                {"message", "user auth with id 5" },
            };

            return true;
        }

        protected override IEnumerable<object> GetEqualityComponentsInternal()
        {
            yield return FormatId;
        }
    }
}
