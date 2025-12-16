using Cleverence.LogTransform.Formats;
using Cleverence.LogTransform.Formats.Builders;
using Cleverence.LogTransform.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cleverence.LogTransform.Formatting
{
    [TestFixture, Category("Integration")]
    internal class LogFormattingIntegrationTests
    {
        #region Format_ValidEntry

        [Test]
        public void Format_LogEntryWithCustomFormatters_ShouldProduceExpectedOutputString()
        {
            var partSet = new LogFormatPartSetBuilder()
                .AddPart("timestamp", LogFormatPartType.DATETIME,
                    parser: s => DateTime.Parse(s),
                    formatter: d => ((DateTime)d!).ToString("yyyy-MM-dd HH:mm:ss"))
                .AddPart("level", LogFormatPartType.TEXT,
                    parser: s => s,
                    formatter: s => $"[{s}]")
                .AddPart("message", LogFormatPartType.TEXT,
                    parser: s => s,
                    formatter: s => s == null ? "(NULL)" : s.ToString().ToUpperInvariant())
                .Build();

            var format = new DelimetedLogFormat(' ', partSet, supportQuotedFields: false);
            var formatter = new LogFormatter();

            var values = new ReadOnlyDictionary<string, object?>(
                new Dictionary<string, object?>
                {
                    ["timestamp"] = new DateTime(2025, 12, 16, 14, 30, 45),
                    ["level"] = "WARN",
                    ["message"] = "disk space low"
                });

            var entry = new LogEntry(format, values);

            var result = formatter.Format(entry);

            Assert.That(result, Is.EqualTo("2025-12-16 14:30:45 [WARN] DISK SPACE LOW"));
        }

        [Test]
        public void Format_LogEntryWithNullValue_ShouldUseFormatterToHandleNullGracefully()
        {
            var partSet = new LogFormatPartSetBuilder()
                .AddPart("id", LogFormatPartType.INTEGER,
                    parser: s => int.Parse(s),
                    formatter: i => $"ID:{i}")
                .AddPart("optional", LogFormatPartType.TEXT,
                    parser: s => s,
                    formatter: s => s == null ? "(empty)" : s.ToString())
                .Build();

            var format = new DelimetedLogFormat('|', partSet, supportQuotedFields: false);
            var formatter = new LogFormatter();

            var values = new ReadOnlyDictionary<string, object?>(
                new Dictionary<string, object?>
                {
                    ["id"] = 42,
                    ["optional"] = null
                });

            var entry = new LogEntry(format, values);

            var result = formatter.Format(entry);

            Assert.That(result, Is.EqualTo("ID:42|(empty)"));
        }

        #endregion

        #region Format_WithErrorHandling

        [Test]
        public void Format_LogEntryWithFailingFormatter_ShouldThrowInvalidOperationException()
        {
            var throwingFormatter = new Func<object?, string>(_ => throw new NotSupportedException("Simulated failure"));
            var partSet = new LogFormatPartSetBuilder()
                .AddPart("safe", LogFormatPartType.TEXT, s => s, s => s?.ToString() ?? "")
                .AddPart("unsafe", LogFormatPartType.TEXT, s => s, throwingFormatter)
                .Build();

            var format = new DelimetedLogFormat(',', partSet, supportQuotedFields: false);
            var formatter = new LogFormatter();

            var values = new ReadOnlyDictionary<string, object?>(
                new Dictionary<string, object?>
                {
                    ["safe"] = "ok",
                    ["unsafe"] = "will-fail"
                });

            var entry = new LogEntry(format, values);

            var ex = Assert.Throws<InvalidOperationException>(() => formatter.Format(entry));
            Assert.That(ex.Message, Does.Contain("Failed to format log entry"));
        }

        #endregion
    }
}
