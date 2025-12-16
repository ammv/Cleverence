using Cleverence.LogTransform.Formats;
using Cleverence.LogTransform.Formats.Builders;
using Cleverence.LogTransform.Models;
using Cleverence.LogTransform.Parsing;
using Cleverence.LogTransform.Transformation;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cleverence.LogTransform.Tests.IntegrationTests.Transformation
{
    [TestFixture, Category("Integration")]
    internal class LogEntryTransformationIntegrationTest
    {
        #region Transform_ValidInput

        [Test]
        public void Transform_SyslogLikeFormatToCompactFormat_ShouldMapFieldsCorrectly()
        {
            // Arrange: входной формат (CSV-подобный)
            var inputPartSet = new LogFormatPartSetBuilder()
                .AddPart("timestamp", LogFormatPartType.DATETIME, s => DateTime.Parse(s))
                .AddPart("level", LogFormatPartType.TEXT, s => s)
                .AddPart("logger", LogFormatPartType.TEXT, s => s)
                .AddPart("message", LogFormatPartType.TEXT, s => s)
                .Build();

            var inputFormat = new DelimetedLogFormat(
                separator: ',',
                partSet: inputPartSet,
                supportQuotedFields: true);

            // Arrange: выходной формат (упрощённый)
            var outputPartSet = new LogFormatPartSetBuilder()
                .AddPart("event", LogFormatPartType.TEXT, s => s)
                .AddPart("ts", LogFormatPartType.DATETIME, s => DateTime.Parse(s))
                .Build();

            var outputFormat = new DelimetedLogFormat(
                separator: '|',
                partSet: outputPartSet,
                supportQuotedFields: false);

            // Arrange: функция преобразования
            var transformerFunc = new Func<LogEntry, LogEntry>(entry =>
            {
                var values = new Dictionary<string, object?>
                {
                    ["event"] = $"{entry.GetStringValue("logger")}: {entry.GetStringValue("message")}",
                    ["ts"] = entry.GetDateTimeValue("timestamp")
                };
                return new LogEntry(outputFormat, new ReadOnlyDictionary<string, object?>(values));
            });

            var boundTransformer = FormatBoundLogTransformer.Create(inputFormat, transformerFunc);
            var transformMap = new LogTransformMapBuilder(outputFormat)
                .Add(inputFormat, boundTransformer)
                .Build();

            var entryTransformer = new LogTransformer(transformMap);

            const string rawInput = "\"2025-12-16T10:00:00\",\"INFO\",\"AuthService\",\"User authenticated\"";

            // Act
            var parser = new LogParser(new[] { inputFormat });
            var parsedEntry = parser.Parse(rawInput);
            var transformedEntry = entryTransformer.Transform(parsedEntry);

            // Assert
            Assert.That(transformedEntry.Format, Is.SameAs(outputFormat));
            Assert.That(transformedEntry.GetStringValue("event"), Is.EqualTo("AuthService: User authenticated"));
            Assert.That(transformedEntry.GetDateTimeValue("ts"), Is.EqualTo(new DateTime(2025, 12, 16, 10, 0, 0)));
        }

        #endregion

        #region Transform_InvalidInput

        [Test]
        public void Transform_EntryWithWrongFormat_ShouldThrowArgumentException()
        {
            var inputFormat = CreateSimpleFormat("input");
            var outputFormat = CreateSimpleFormat("output");

            var transformerFunc = new Func<LogEntry, LogEntry>(e => new LogEntry(outputFormat, e.Values));
            var boundTransformer = FormatBoundLogTransformer.Create(inputFormat, transformerFunc);
            var transformMap = new LogTransformMapBuilder(outputFormat)
                .Add(inputFormat, boundTransformer)
                .Build();

            var entryTransformer = new LogTransformer(transformMap);

            var wrongFormat = CreateSimpleFormat("wrong");
            var wrongEntry = new LogEntry(
                wrongFormat, 
                new ReadOnlyDictionary<string, object?>(
                    new Dictionary<string, object?>()
                    {
                        ["wrong"] = "someText"
                    }));

            var ex = Assert.Throws<ArgumentException>(() => entryTransformer.Transform(wrongEntry));
            Assert.That(ex.Message, Does.Contain("Log entry format"));
            Assert.That(ex.Message, Does.Contain("does not present in transformation map."));
        }

        #endregion

        private static ILogFormat CreateSimpleFormat(string partName)
        {
            var partSet = new LogFormatPartSetBuilder()
                .AddPart(partName, new LogFormatPartDefault(LogFormatPartType.TEXT, s => s))
                .Build();
            return new DelimetedLogFormat(',', partSet, false);
        }
    }
}
