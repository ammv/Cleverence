using Cleverence.LogTransform.Exceptions;
using Cleverence.LogTransform.Formats;
using Cleverence.LogTransform.Formats.Builders;
using Cleverence.LogTransform.Models;
using Cleverence.LogTransform.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cleverence.LogTransform.Tests.IntegrationTests.Parsing
{
    [TestFixture, Category("Integration")]
    internal class RegexLogFormatParsingIntegrationTest
    {
        #region Parse_ValidInput

        [Test]
        public void Parse_ValidLogLine_ShouldProduceStructuredLogEntry_WithCorrectValues()
        {
            var partSet = new LogFormatPartSetBuilder()
                .AddPart("timestamp", LogFormatPartType.DATETIME, s => DateTime.Parse(s))
                .AddPart("level", LogFormatPartType.TEXT, s => s)
                .AddPart("message", LogFormatPartType.TEXT, s => s)
                .Build();

            var pattern = @"(?<timestamp>\d{4}\-\d{2}\-\d{2}T\d{2}:\d{2}:\d{2}),(?<level>\w+),(?<message>.+)";
            var format = new RegexLogFormat(',', pattern, partSet);
            var parser = new LogParser(new[] { format });

            const string logLine = "2025-12-16T08:30:00,INFO,User logged in";

            var entry = parser.Parse(logLine);

            Assert.That(entry.Format, Is.SameAs(format));
            Assert.That(entry.GetDateTimeValue("timestamp"), Is.EqualTo(new DateTime(2025, 12, 16, 8, 30, 0)));
            Assert.That(entry.GetStringValue("level"), Is.EqualTo("INFO"));
            Assert.That(entry.GetStringValue("message"), Is.EqualTo("User logged in"));
        }

        [Test]
        public void Parse_ValidWithEmptyMessage_ShouldParseAsEmptyString()
        {
            var partSet = new LogFormatPartSetBuilder()
                .AddPart("timestamp", LogFormatPartType.DATETIME, s => DateTime.Parse(s))
                .AddPart("level", LogFormatPartType.TEXT, s => s)
                .AddPart("message", LogFormatPartType.TEXT, s => s)
                .Build();

            var pattern = @"(?<timestamp>\d{4}\-\d{2}\-\d{2}T\d{2}:\d{2}:\d{2}),(?<level>\w+),(?<message>.{0,})";
            var format = new RegexLogFormat(',', pattern, partSet);
            var parser = new LogParser(new[] { format });

            const string logLine = "2025-12-16T08:30:00,INFO,";

            var entry = parser.Parse(logLine);

            Assert.That(entry.Format, Is.SameAs(format));
            Assert.That(entry.GetDateTimeValue("timestamp"), Is.EqualTo(new DateTime(2025, 12, 16, 8, 30, 0)));
            Assert.That(entry.GetStringValue("level"), Is.EqualTo("INFO"));
            Assert.That(entry.GetStringValue("message"), Is.EqualTo(String.Empty));
        }

        #endregion

        #region Parse_InvalidInput

        [Test]
        public void Parse_UnparsableDateTimeInLog_ShouldThrowLogParseException()
        {
            var partSet = new LogFormatPartSetBuilder()
                .AddPart("date", LogFormatPartType.DATETIME, s => DateTime.Parse(s))
                .AddPart("event", LogFormatPartType.TEXT, s => s)
                .Build();

            var pattern = @"(?<date>\d{4}\-\d{2}\-\d{2}),(?<event>.+)";
            var format = new RegexLogFormat(',', pattern, partSet);
            var parser = new LogParser(new[] { format });

            const string logLine = "not-a-date,system-start";

            var ex = Assert.Throws<LogParseException>(() => parser.Parse(logLine));
            Assert.That(ex.Message, Does.Contain("Failed to parse log line"));
        }

        [Test]
        public void Parse_MismatchedFieldCount_ShouldThrowLogParseException()
        {
            var partSet = new LogFormatPartSetBuilder()
                .AddPart("a", LogFormatPartType.TEXT, s => s)
                .AddPart("b", LogFormatPartType.TEXT, s => s)
                .Build();

            var pattern = @"(?<a>\d{4}\-\d{2}\-\d{2}),(?<b>.+)";
            var format = new RegexLogFormat('|', pattern, partSet);
            var parser = new LogParser(new[] { format });

            const string logLine = "only-one-field";

            var ex = Assert.Throws<LogParseException>(() => parser.Parse(logLine));
            Assert.That(ex.Message, Does.Contain("No registered format matches the input."));
        }

        #endregion
    }
}
