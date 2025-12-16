using Cleverence.LogTransform.Formats;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cleverence.LogTransform.Tests.Helpers
{
    internal class RegexLogFormatFactory
    {
        internal static RegexLogFormat CreateValidRegexFormat(MockLogFormatPart mockPart = null)
        {
            mockPart = mockPart ?? new MockLogFormatPart();
            var parts = new ReadOnlyDictionary<string, ILogFormatPart>(
                new Dictionary<string, ILogFormatPart> { ["field"] = mockPart });
            return new RegexLogFormat(
                separator: ' ',
                pattern: @"(?<field>\w+)",
                partNames: new[] { "field" },
                parts: parts);
        }
    }
}
