using Cleverence.LogTransform.Exceptions;
using Cleverence.LogTransform.Formats;
using Cleverence.LogTransform.Parsing;
using Cleverence.LogTransform.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cleverence.LogTransform.Tests.UnitTests.Parsing
{
    internal class LogParsingUnitTest
    {
        #region Constructor

        [Test]
        public void Constructor_ShouldThrowArgumentNullException_WhenSupportedFormatsIsNull()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new LogParser(null!));
            Assert.That(ex.ParamName, Is.EqualTo("supportedFormats"));
        }

        [Test]
        public void Constructor_ShouldThrowArgumentException_WhenSupportedFormatsIsEmpty()
        {
            var formats = Array.Empty<ILogFormat>();
            var ex = Assert.Throws<ArgumentException>(() => new LogParser(formats));
            Assert.That(ex.ParamName, Is.EqualTo("supportedFormats"));
            Assert.That(ex.Message, Does.Contain("At least one log format must be provided."));
        }

        [Test]
        public void Constructor_ShouldInitialize_WhenSupportedFormatsIsNonEmpty()
        {
            var format = new MockLogFormat(new[] { "f" },
                new ReadOnlyDictionary<string, ILogFormatPart>(new Dictionary<string, ILogFormatPart> { ["f"] = new MockLogFormatPart() }));
            var formats = new[] { format };

            var parser = new LogParser(formats);

            Assert.That(parser, Is.Not.Null);
        }

        #endregion

        #region Parse

        [Test]
        public void Parse_ShouldThrowArgumentNullException_WhenLogLineIsNull()
        {
            var format = MockLogFormatFactory.CreateFailingMockFormat();
            var parser = new LogParser(new[] { format });

            var ex = Assert.Throws<ArgumentNullException>(() => parser.Parse(null!));
            Assert.That(ex.ParamName, Is.EqualTo("logLine"));
        }

        [Test]
        public void Parse_ShouldThrowArgumentException_WhenLogLineIsEmpty()
        {
            var format = MockLogFormatFactory.CreateFailingMockFormat();
            var parser = new LogParser(new[] { format });

            var ex = Assert.Throws<ArgumentException>(() => parser.Parse(""));
            Assert.That(ex.ParamName, Is.EqualTo("logLine"));
        }

        [Test]
        public void Parse_ShouldThrowLogParseException_WhenNoFormatMatches()
        {
            var format = MockLogFormatFactory.CreateFailingMockFormat();

            format.TryParseFuncOverride = (string x, out IReadOnlyDictionary<string, object?> y) => { y = default; return false; };

            var parser = new LogParser(new[] { format });

            var ex = Assert.Throws<LogParseException>(() => parser.Parse("unparsable line"));
            Assert.That(ex.Message, Does.Contain("Failed to parse log line"));
            Assert.That(ex.Message, Does.Contain("No registered format matches the input."));
        }

        [Test]
        public void Parse_ShouldReturnLogEntry_WhenFirstFormatSucceeds()
        {
            var successFormat = MockLogFormatFactory.CreateSuccessfulMockFormat("valid", new Dictionary<string, object?> { ["x"] = "parsed" });
            var failingFormat = MockLogFormatFactory.CreateFailingMockFormat();
            var parser = new LogParser(new[] { successFormat, failingFormat });

            var entry = parser.Parse("valid");

            Assert.That(entry, Is.Not.Null);
            Assert.That(entry.Format, Is.SameAs(successFormat));
            Assert.That(entry.GetRawValue("x"), Is.EqualTo("parsed"));
        }

        [Test]
        public void Parse_ShouldUseFirstMatchingFormat_WhenMultipleFormatsCouldMatch()
        {
            var firstFormat = MockLogFormatFactory.CreateSuccessfulMockFormat("input", new Dictionary<string, object?> { ["a"] = "first" });
            var secondFormat = MockLogFormatFactory.CreateSuccessfulMockFormat("input", new Dictionary<string, object?> { ["b"] = "second" });
            var parser = new LogParser(new[] { firstFormat, secondFormat });

            var entry = parser.Parse("input");

            Assert.That(entry.Format, Is.SameAs(firstFormat));
            Assert.That(entry.GetRawValue("a"), Is.EqualTo("first"));
        }

        [Test]
        public void Parse_ShouldTruncateLongLogLineInExceptionMessage()
        {
            var format = MockLogFormatFactory.CreateFailingMockFormat();

            format.TryParseFuncOverride = (string x, out IReadOnlyDictionary<string, object?> y) => { y = default; return false; };

            var parser = new LogParser(new[] { format });

            var longLine = new string('x', 150);
            var ex = Assert.Throws<LogParseException>(() => parser.Parse(longLine));

            Assert.That(ex.Message, Does.Contain("xxx..."));
            Assert.That(ex.Message.Length, Is.LessThanOrEqualTo(200)); // грубо, но проверяем, что обрезано
        }

        #endregion

        #region TryParse

        [Test]
        public void TryParse_ShouldReturnFalse_WhenLogLineIsNull()
        {
            var format = MockLogFormatFactory.CreateFailingMockFormat();
            var parser = new LogParser(new[] { format });

            var success = parser.TryParse(null!, out var entry);

            Assert.That(success, Is.False);
            Assert.That(entry, Is.Null);
        }

        [Test]
        public void TryParse_ShouldReturnFalse_WhenLogLineIsEmpty()
        {
            var format = MockLogFormatFactory.CreateFailingMockFormat();

            var parser = new LogParser(new[] { format });

            var success = parser.TryParse("", out var entry);

            Assert.That(success, Is.False);
            Assert.That(entry, Is.Null);
        }

        [Test]
        public void TryParse_ShouldReturnFalse_WhenNoFormatMatches()
        {
            var format = MockLogFormatFactory.CreateFailingMockFormat();

            format.TryParseFuncOverride = (string x, out IReadOnlyDictionary<string, object?> y) => { y = default; return false; };

            var parser = new LogParser(new[] { format });

            var success = parser.TryParse("no match", out var entry);

            Assert.That(success, Is.False);
            Assert.That(entry, Is.Null);
        }

        [Test]
        public void TryParse_ShouldReturnTrueAndLogEntry_WhenFormatMatches()
        {
            var values = new Dictionary<string, object?> { ["id"] = 42 };
            var format = MockLogFormatFactory.CreateSuccessfulMockFormat("data", values);
            var parser = new LogParser(new[] { format });

            var success = parser.TryParse("data", out var entry);

            Assert.That(success, Is.True);
            Assert.That(entry, Is.Not.Null);
            Assert.That(entry.Format, Is.SameAs(format));
            Assert.That(entry.GetRawValue("id"), Is.EqualTo(42));
        }

        [Test]
        public void TryParse_ShouldIterateFormatsInOrderUntilFirstMatch()
        {
            FuncWithOut<string, IReadOnlyDictionary<string, object?>, bool> tryParseFuncOverride =
                (string x, out IReadOnlyDictionary<string, object?> y) => { y = default; return false; };

            var failing1 = MockLogFormatFactory.CreateFailingMockFormat();
            var success = MockLogFormatFactory.CreateSuccessfulMockFormat("ok", new Dictionary<string, object?> { ["v"] = "success" });
            var failing2 = MockLogFormatFactory.CreateFailingMockFormat();

            failing1.TryParseFuncOverride = tryParseFuncOverride;
            failing2.TryParseFuncOverride = tryParseFuncOverride;

            var parser = new LogParser(new[] { failing1, success, failing2 });

            var successResult = parser.TryParse("ok", out var entry);

            Assert.That(successResult, Is.True);
            Assert.That(entry.Format, Is.SameAs(success));
        }

        #endregion
    }
}
