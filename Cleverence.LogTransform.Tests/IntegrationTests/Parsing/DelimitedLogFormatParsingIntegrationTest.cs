using Cleverence.LogTransform.Exceptions;
using Cleverence.LogTransform.Formats;
using Cleverence.LogTransform.Formats.Builders;
using Cleverence.LogTransform.Models;
using Cleverence.LogTransform.Parsing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cleverence.LogTransform.Tests.IntegrationTests.Parsing
{
    [TestFixture, Category("Integration")]
    internal class DelimitedLogFormatParsingIntegrationTest
    {
        #region Parse_ValidInput

        [Test]
        public void Parse_ValidCsvLogLine_ShouldProduceStructuredLogEntry_WithCorrectValues()
        {
            var partSet = new LogFormatPartSetBuilder()
                .AddPart("timestamp", LogFormatPartType.DATETIME, s => DateTime.Parse(s))
                .AddPart("level", LogFormatPartType.TEXT, s => s)
                .AddPart("message", LogFormatPartType.TEXT, s => s)
                .Build();

            var format = new DelimetedLogFormat(',', partSet, supportQuotedFields: true);
            var parser = new LogParser(new[] { format });

            const string logLine = "\"2025-12-16T08:30:00\",\"INFO\",\"User logged in\"";

            var entry = parser.Parse(logLine);

            Assert.That(entry.Format, Is.SameAs(format));
            Assert.That(entry.GetDateTimeValue("timestamp"), Is.EqualTo(new DateTime(2025, 12, 16, 8, 30, 0)));
            Assert.That(entry.GetStringValue("level"), Is.EqualTo("INFO"));
            Assert.That(entry.GetStringValue("message"), Is.EqualTo("User logged in"));
        }

        [Test]
        public void Parse_ValidCsvWithEmptyQuotedField_ShouldParseAsEmptyString()
        {
            var partSet = new LogFormatPartSetBuilder()
                .AddPart("id", LogFormatPartType.INTEGER, s => int.Parse(s))
                .AddPart("name", LogFormatPartType.TEXT, s => s)
                .AddPart("note", LogFormatPartType.TEXT, s => s)
                .Build();

            var format = new DelimetedLogFormat(',', partSet, supportQuotedFields: true);
            var parser = new LogParser(new[] { format });

            const string logLine = "123,\"john\",\"\"";

            var entry = parser.Parse(logLine);

            Assert.That(entry.GetIntegerValue("id"), Is.EqualTo(123));
            Assert.That(entry.GetStringValue("name"), Is.EqualTo("john"));
            Assert.That(entry.GetStringValue("note"), Is.EqualTo(string.Empty));
        }

        [Test]
        public void Parse_DelimitedLogWithTrailingMessageField_ShouldTreatLastPartAsRemainingString()
        {
            var partSet = new LogFormatPartSetBuilder()
                .AddPart("date", LogFormatPartType.DATETIME, s => DateTime.Parse(s))
                .AddPart("time", LogFormatPartType.DATETIME, s => DateTime.Parse(s).TimeOfDay)
                .AddPart("level", LogFormatPartType.TEXT, s => s)
                .AddPart("message", LogFormatPartType.TEXT, s => s)
                .Build();

            var format = new DelimetedLogFormat(' ', partSet, supportQuotedFields: false);
            var parser = new LogParser(new[] { format });

            const string logLine = "10.03.2025 15:14:49.523 INFORMATION Program version: '3.4.0.48729'";

            var entry = parser.Parse(logLine);

            Assert.That(entry.GetDateTimeValue("date"), Is.EqualTo(new DateTime(2025, 3, 10)));
            Assert.That(entry.GetValue<TimeSpan>("time"), Is.EqualTo(new TimeSpan(0,15,14,49,523)));
            Assert.That(entry.GetStringValue("level"), Is.EqualTo("INFORMATION"));
            Assert.That(entry.GetStringValue("message"), Is.EqualTo("Program version: '3.4.0.48729'"));
        }

        #endregion

        #region Parse_InvalidInput

        [Test]
        public void Parse_UnparsableDateTimeInCsvLog_ShouldThrowLogParseException()
        {
            var partSet = new LogFormatPartSetBuilder()
                .AddPart("date", LogFormatPartType.DATETIME, s => DateTime.Parse(s))
                .AddPart("event", LogFormatPartType.TEXT, s => s)
                .Build();

            var format = new DelimetedLogFormat(',', partSet, false);
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

            var format = new DelimetedLogFormat('|', partSet, false);
            var parser = new LogParser(new[] { format });

            const string logLine = "only-one-field";

            var ex = Assert.Throws<LogParseException>(() => parser.Parse(logLine));
            Assert.That(ex.Message, Does.Contain("No registered format matches the input."));
        }

        #endregion
    }
}
