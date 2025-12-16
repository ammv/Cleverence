using Cleverence.LogTransform.Formats;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cleverence.LogTransform.Tests.Helpers
{
    internal class MockLogFormatFactory
    {
        internal static MockLogFormat CreateFailingMockFormat()
        {
            return new MockLogFormat(
                new[] { "dummy" },
                new ReadOnlyDictionary<string, ILogFormatPart>(
                    new Dictionary<string, ILogFormatPart>
                    {
                        ["dummy"] = new MockLogFormatPart(parser: _ => null)
                    }));
        }

        internal static MockLogFormat CreateSuccessfulMockFormat(string expectedInput, Dictionary<string, object?> outputValues)
        {
            var partNames = new List<string>(outputValues.Keys);
            var parts = new Dictionary<string, ILogFormatPart>();
            foreach (var name in partNames)
            {
                var capturedValue = outputValues[name];
                parts[name] = new MockLogFormatPart(parser: _ => capturedValue);
            }

            var mock = new MockLogFormat(partNames.ToArray(), new ReadOnlyDictionary<string, ILogFormatPart>(parts));

            mock.TryParseFuncOverride = (string input, out IReadOnlyDictionary<string, object?> values) =>
            {
                if (input == expectedInput)
                {
                    values = new ReadOnlyDictionary<string, object?>(outputValues);
                    return true;
                }
                values = null;
                return false;
            };

            return mock;
        }
    }
}
