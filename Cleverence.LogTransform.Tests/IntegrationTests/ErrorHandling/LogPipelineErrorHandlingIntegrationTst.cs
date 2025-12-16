using Cleverence.LogTransform.Exceptions;
using Cleverence.LogTransform.Formats;
using Cleverence.LogTransform.Formatting;
using Cleverence.LogTransform.Models;
using Cleverence.LogTransform.Parsing;
using Cleverence.LogTransform.Transformation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cleverence.LogTransform.Tests.IntegrationTests.ErrorHandling
{
    [TestFixture, Category("Integration")]
    public class LogPipelineErrorHandlingIntegrationTests
    {
        #region Parse_Errors

        [Test]
        public void Parse_NullLogLine_ShouldThrowArgumentException()
        {
            var format = CreateSimpleFormat();
            var parser = new LogParser(new[] { format });

            var ex = Assert.Throws<ArgumentNullException>(() => parser.Parse(null!));
            Assert.That(ex.ParamName, Is.EqualTo("logLine"));
            Assert.That(ex.Message, Does.Contain("Value cannot be null"));
        }

        [Test]
        public void Parse_EmptyLogLine_ShouldThrowArgumentException()
        {
            var format = CreateSimpleFormat();
            var parser = new LogParser(new[] { format });

            var ex = Assert.Throws<ArgumentException>(() => parser.Parse(""));
            Assert.That(ex.ParamName, Is.EqualTo("logLine"));
            Assert.That(ex.Message, Does.Contain("The value cannot be an empty string"));
        }

        [Test]
        public void Parse_UnparsableLogLine_ShouldThrowLogParseException_WithTruncatedMessage()
        {
            var format = CreateSimpleFormat(parserThrowException: true);
            var parser = new LogParser(new[] { format });

            var longLine = new string('x', 150);
            var ex = Assert.Throws<LogParseException>(() => parser.Parse(longLine));

            Assert.That(ex.Message, Does.Contain("Failed to parse log line"));
            Assert.That(ex.Message, Does.Contain("xxx..."));
            Assert.That(ex.Message, Does.Not.Contain(longLine)); // убедимся, что обрезано
        }

        [Test]
        public void Parse_NoMatchingFormat_ShouldThrowLogParseException()
        {
            var format = CreateFailingFormat(true);
            var parser = new LogParser(new[] { format });

            var ex = Assert.Throws<LogParseException>(() => parser.Parse("any input"));
            Assert.That(ex.Message, Does.Contain("No registered format matches the input."));
        }

        #endregion

        #region Transform_Errors

        [Test]
        public void Transform_NullEntry_ShouldThrowArgumentNullException()
        {
            var inputFormat = CreateSimpleFormat();
            var outputFormat = CreateSimpleFormat();
            var transformer = CreateIdentityTransformer(inputFormat, outputFormat);
            var map = new LogTransformMap(
                outputFormat,
                new ReadOnlyDictionary<ILogFormat, FormatBoundLogTransformer>(
                    new Dictionary<ILogFormat, FormatBoundLogTransformer> { [inputFormat] = transformer }));
            var entryTransformer = new LogTransformer(map);

            var ex = Assert.Throws<ArgumentNullException>(() => entryTransformer.Transform(null!));
            Assert.That(ex.ParamName, Is.EqualTo("entry"));
        }

        [Test]
        public void Transform_EntryWithMismatchedFormat_ShouldThrowArgumentException()
        {
            var expectedFormat = CreateSimpleFormat("expected");
            var actualFormat = CreateSimpleFormat("actual");
            var outputFormat = CreateSimpleFormat();
            var transformer = CreateIdentityTransformer(expectedFormat, outputFormat);
            var map = new LogTransformMap(
                outputFormat,
                new ReadOnlyDictionary<ILogFormat, FormatBoundLogTransformer>(
                    new Dictionary<ILogFormat, FormatBoundLogTransformer> { [expectedFormat] = transformer }));
            var entryTransformer = new LogTransformer(map);

            var wrongEntry = new LogEntry(actualFormat, 
                new ReadOnlyDictionary<string, object?>(
                    new Dictionary<string, object?>()
                    {["actual"] = "123" }));

            var ex = Assert.Throws<ArgumentException>(() => entryTransformer.Transform(wrongEntry));
            Assert.That(ex.ParamName, Is.EqualTo("entry"));
            Assert.That(ex.Message, Does.Contain("Log entry format"));
            Assert.That(ex.Message, Does.Contain("does not present in transformation map."));
        }

        #endregion

        #region Format_Errors

        [Test]
        public void Format_NullEntry_ShouldThrowArgumentNullException()
        {
            var formatter = new LogFormatter();

            var ex = Assert.Throws<ArgumentNullException>(() => formatter.Format(null!));
            Assert.That(ex.ParamName, Is.EqualTo("entry"));
        }

        #endregion

        // Вспомогательные фабрики

        private static ILogFormat CreateSimpleFormat(string partName = "field", bool parserThrowException = false)
        {
            Func<object?, string> parser = parserThrowException ? _ => throw new Exception("Parsing exception") : s => s.ToString();
            var partSet = new LogFormatPartSet(
                new[] { partName },
                new ReadOnlyDictionary<string, ILogFormatPart>(
                    new Dictionary<string, ILogFormatPart>
                    {
                        [partName] = new LogFormatPartDefault(LogFormatPartType.TEXT, parser)
                    }));
            return new DelimetedLogFormat(',', partSet, false);
        }

        private static ILogFormat CreateFailingFormat(bool parserThrowException = false)
        {
            Func<object?, string> parser = parserThrowException ? _ => throw new Exception("Parsing exception") : _ => null;
            var partSet = new LogFormatPartSet(
                new[] { "x" },
                new ReadOnlyDictionary<string, ILogFormatPart>(
                    new Dictionary<string, ILogFormatPart>
                    {
                        ["x"] = new LogFormatPartDefault(LogFormatPartType.TEXT, parser)
                    }));
            return new DelimetedLogFormat(',', partSet, false);
        }

        private static FormatBoundLogTransformer CreateIdentityTransformer(ILogFormat input, ILogFormat output)
        {
            return FormatBoundLogTransformer.Create(input, entry => new LogEntry(output, entry.Values));
        }
    }
}
