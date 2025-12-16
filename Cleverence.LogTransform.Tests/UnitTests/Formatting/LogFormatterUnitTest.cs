using Cleverence.LogTransform.Formats;
using Cleverence.LogTransform.Formatting;
using Cleverence.LogTransform.Tests.Helpers;
using System.Collections.ObjectModel;

namespace Cleverence.LogTransform.Tests.UnitTests.Formatting
{
    [TestFixture]
    internal class LogFormatterUnitTest
    {
        #region TryFormat

        [Test]
        public void TryFormat_ShouldThrowArgumentNullException_WhenEntryIsNull()
        {
            var formatter = new LogFormatter();

            var ex = Assert.Throws<ArgumentNullException>(() => formatter.TryFormat(null!, out _));
            Assert.That(ex.ParamName, Is.EqualTo("entry"));
        }

        [Test]
        public void TryFormat_ShouldReturnTrueAndFormattedString_WhenAllFormattersSucceed()
        {
            var part1 = new MockLogFormatPart(formatter: o => $"[{o}]");
            var part2 = new MockLogFormatPart(formatter: o => o?.ToString().ToUpperInvariant() ?? "NULL");

            var format = new MockLogFormat(
                '|',
                new[] { "id", "message" },
                new Dictionary<string, ILogFormatPart>
                {
                    ["id"] = part1,
                    ["message"] = part2
                });

            var values = new ReadOnlyDictionary<string, object?>(
                new Dictionary<string, object?>
                {
                    ["id"] = 42,
                    ["message"] = "hello"
                });

            var entry = LogEntryFactory.CreateLogEntry(format, values);
            var formatter = new LogFormatter();

            var success = formatter.TryFormat(entry, out var result);

            Assert.That(success, Is.True);
            Assert.That(result, Is.EqualTo("[42]|HELLO"));
        }

        [Test]
        public void TryFormat_ShouldReturnTrueAndUseSeparatorFromFormat()
        {
            var part = new MockLogFormatPart(formatter: o => "X");
            var format = new MockLogFormat(
                '→',
                new[] { "a", "b" },
                new Dictionary<string, ILogFormatPart> { ["a"] = part, ["b"] = part });

            var values = new ReadOnlyDictionary<string, object?>(
                new Dictionary<string, object?> { ["a"] = null, ["b"] = null });
            var entry = LogEntryFactory.CreateLogEntry(format, values);
            var formatter = new LogFormatter();

            var success = formatter.TryFormat(entry, out var result);

            Assert.That(success, Is.True);
            Assert.That(result, Is.EqualTo("X→X"));
        }

        [Test]
        public void TryFormat_ShouldReturnFalse_WhenAnyFormatterThrows()
        {
            var part1 = new MockLogFormatPart(formatter: o => "ok");
            var part2 = new MockLogFormatPart(formatter: _ => throw new InvalidOperationException("Boom!"));

            var format = new MockLogFormat(
                new[] { "good", "bad" },
                new Dictionary<string, ILogFormatPart>
                {
                    ["good"] = part1,
                    ["bad"] = part2
                });

            var values = new ReadOnlyDictionary<string, object?>(
                new Dictionary<string, object?> { ["good"] = 1, ["bad"] = 2 });
            var entry = LogEntryFactory.CreateLogEntry(format, values);
            var formatter = new LogFormatter();

            var success = formatter.TryFormat(entry, out var result);

            Assert.That(success, Is.False);
            Assert.That(result, Is.Null);
        }

        [Test]
        public void TryFormat_ShouldHandleNullValuesCorrectly_WhenFormatterSupportsNull()
        {
            var part = new MockLogFormatPart(formatter: o => o == null ? "(null)" : o.ToString()!);
            var format = new MockLogFormat(
                new[] { "field" },
                new Dictionary<string, ILogFormatPart> { ["field"] = part });

            var values = new ReadOnlyDictionary<string, object?>(
                new Dictionary<string, object?> { ["field"] = null });
            var entry = LogEntryFactory.CreateLogEntry(format, values);
            var formatter = new LogFormatter();

            var success = formatter.TryFormat(entry, out var result);

            Assert.That(success, Is.True);
            Assert.That(result, Is.EqualTo("(null)"));
        }

        [Test]
        public void TryFormat_ShouldPreserveOrderOfPartNames_WhenFormatting()
        {
            var p1 = new MockLogFormatPart(formatter: _ => "FIRST");
            var p2 = new MockLogFormatPart(formatter: _ => "SECOND");
            var p3 = new MockLogFormatPart(formatter: _ => "THIRD");

            // Order: z, x, y
            var format = new MockLogFormat(
                ',',
                new[] { "z", "x", "y" },
                new Dictionary<string, ILogFormatPart>
                {
                    ["x"] = p2,
                    ["y"] = p3,
                    ["z"] = p1
                });

            var values = new ReadOnlyDictionary<string, object?>(
                new Dictionary<string, object?>
                {
                    ["x"] = null,
                    ["y"] = null,
                    ["z"] = null
                });
            var entry = LogEntryFactory.CreateLogEntry(format, values);
            var formatter = new LogFormatter();

            var success = formatter.TryFormat(entry, out var result);

            Assert.That(success, Is.True);
            Assert.That(result, Is.EqualTo("FIRST,SECOND,THIRD"));
        }

        #endregion

        #region Format

        [Test]
        public void Format_ShouldThrowArgumentNullException_WhenEntryIsNull()
        {
            var formatter = new LogFormatter();

            var ex = Assert.Throws<ArgumentNullException>(() => formatter.Format(null!));
            Assert.That(ex.ParamName, Is.EqualTo("entry"));
        }

        [Test]
        public void Format_ShouldReturnFormattedString_WhenAllFormattersSucceed()
        {
            var part = new MockLogFormatPart(formatter: o => $"VAL:{o}");
            var format = new MockLogFormat(
                ';',
                new[] { "a", "b" },
                new Dictionary<string, ILogFormatPart> { ["a"] = part, ["b"] = part });

            var values = new ReadOnlyDictionary<string, object?>(
                new Dictionary<string, object?> { ["a"] = 1, ["b"] = 2 });
            var entry = LogEntryFactory.CreateLogEntry(format, values);
            var formatter = new LogFormatter();

            var result = formatter.Format(entry);

            Assert.That(result, Is.EqualTo("VAL:1;VAL:2"));
        }

        [Test]
        public void Format_ShouldThrowInvalidOperationException_WhenAnyFormatterFails()
        {
            var part1 = new MockLogFormatPart(formatter: _ => "ok");
            var part2 = new MockLogFormatPart(formatter: _ => throw new FormatException("Fail"));

            var format = new MockLogFormat(
                new[] { "safe", "unsafe" },
                new Dictionary<string, ILogFormatPart>
                {
                    ["safe"] = part1,
                    ["unsafe"] = part2
                });

            var values = new ReadOnlyDictionary<string, object?>(
                new Dictionary<string, object?> { ["safe"] = 0, ["unsafe"] = 0 });
            var entry = LogEntryFactory.CreateLogEntry(format, values);
            var formatter = new LogFormatter();

            var ex = Assert.Throws<InvalidOperationException>(() => formatter.Format(entry));
            Assert.That(ex.Message, Does.Contain("Failed to format log entry"));
        }

        #endregion
    }
}
