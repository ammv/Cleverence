using Cleverence.LogTransform.Formats;
using Cleverence.LogTransform.Formats.Builders;
using Cleverence.LogTransform.Formatting;
using Cleverence.LogTransform.Models;
using Cleverence.LogTransform.Parsing;
using Cleverence.LogTransform.Transformation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cleverence.LogTransform.ConsoleApp
{
    /// <summary>
    /// Provides end-to-end transformation of log lines from supported input formats to a unified output format.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This service supports two specific input log formats:
    /// <list type="number">
    ///   <item>Space-delimited format with separate date and time fields.</item>
    ///   <item>Pipe-delimited format with combined datetime, thread ID, and calling method.</item>
    /// </list>
    /// All input formats are converted to a single tab-separated output format with normalized fields.
    /// </para>
    /// <para>
    /// The transformation pipeline consists of:
    /// <list type="bullet">
    ///   <item><see cref="LogParser"/> – parses raw log lines into structured <see cref="LogEntry"/> instances.</item>
    ///   <item><see cref="LogTransformer"/> – converts entries from input formats to the unified output format.</item>
    ///   <item><see cref="LogFormatter"/> – renders transformed entries as formatted strings.</item>
    /// </list>
    /// </para>
    /// <para>
    /// This class is stateless after construction and thread-safe.
    /// </para>
    /// </remarks>
    public class LogTransformationService
    {
        private readonly LogParser _parser;
        private readonly LogTransformer _transformer;
        private readonly LogFormatter _formatter;

        private readonly static Func<string, object?> _stubParser = s => s;

        /// <summary>
        /// Initializes a new instance with predefined input and output log formats.
        /// </summary>
        public LogTransformationService()
        {
            var inputFormat1 = CreateInputLogFormat1();
            var inputFormat2 = CreateInputLogFormat2();
            var outputFormat = CreateOutputLogFormat();

            var transformMap = new LogTransformMapBuilder(outputFormat)
                .Add(inputFormat1, CreateTransformerForInputLogFormat1(inputFormat1, outputFormat))
                .Add(inputFormat2, CreateTransformerForInputLogFormat2(inputFormat2, outputFormat))
                .Build();

            _parser = new LogParser(new[] { inputFormat1, inputFormat2 });
            _transformer = new LogTransformer(transformMap);
            _formatter = new LogFormatter();
        }

        private FormatBoundLogTransformer CreateTransformerForInputLogFormat1(
            DelimetedLogFormat inputFormat,
            DelimetedLogFormat outputFormat)
        {
            var transform = new Func<LogEntry, LogEntry>(entry =>
            {
                var values = new Dictionary<string, object?>
                {
                    ["date"] = entry.GetDateTimeValue("date"),
                    ["time"] = entry.GetDateTimeValue("time"),
                    ["level"] = entry.GetValue<LogLevel>("level"),
                    ["callingMethod"] = null,
                    ["message"] = entry.GetStringValue("message")
                };
                return new LogEntry(outputFormat, new ReadOnlyDictionary<string, object?>(values));
            });

            return FormatBoundLogTransformer.Create(inputFormat, transform);
        }

        private FormatBoundLogTransformer CreateTransformerForInputLogFormat2(
            DelimetedLogFormat inputFormat,
            DelimetedLogFormat outputFormat)
        {
            var transform = new Func<LogEntry, LogEntry>(entry =>
            {
                var values = new Dictionary<string, object?>
                {
                    ["date"] = entry.GetDateTimeValue("datetime"),
                    ["time"] = entry.GetDateTimeValue("datetime"),
                    ["level"] = entry.GetValue<LogLevel>("level"),
                    ["callingMethod"] = entry.GetStringValue("callingMethod"),
                    ["message"] = entry.GetStringValue("message")
                };
                return new LogEntry(outputFormat, new ReadOnlyDictionary<string, object?>(values));
            });

            return FormatBoundLogTransformer.Create(inputFormat, transform);
        }

        /// <summary>
        /// Transforms a single raw log line into the unified output format.
        /// </summary>
        /// <param name="logLine">The raw log line to transform. Must conform to one of the supported input formats.</param>
        /// <returns>The transformed log line in tab-separated format.</returns>
        /// <exception cref="LogParseException">
        /// Thrown if the input line does not match any supported format.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if transformation or formatting fails unexpectedly.
        /// </exception>
        public string TransformLine(string logLine)
        {
            var entry = _parser.Parse(logLine);
            var transformed = _transformer.Transform(entry);
            return _formatter.Format(transformed);
        }

        private static DelimetedLogFormat CreateInputLogFormat1()
        {
            var partSet = new LogFormatPartSetBuilder()
                .AddPart("date", Models.LogFormatPartType.DATETIME, s => DateTime.ParseExact(s, "dd.MM.yyyy", CultureInfo.InvariantCulture))
                .AddPart("time", Models.LogFormatPartType.DATETIME, s => DateTime.ParseExact(s, "HH:mm:ss.fff", CultureInfo.InvariantCulture))
                .AddPart("level", Models.LogFormatPartType.OTHER, s => LogLevelUtils.StringToEnum(s))
                .AddPart("message", Models.LogFormatPartType.TEXT, _stubParser)
                .Build();

            return new DelimetedLogFormat(' ', partSet, false);
        }

        private static DelimetedLogFormat CreateInputLogFormat2()
        {
            var partSet = new LogFormatPartSetBuilder()
                .AddPart("datetime", Models.LogFormatPartType.DATETIME,
                    s => DateTime.ParseExact(s, "yyyy-MM-dd HH:mm:ss.ffff", CultureInfo.InvariantCulture))
                .AddPart("level", Models.LogFormatPartType.OTHER, s => LogLevelUtils.StringToEnum(s))
                .AddPart("threadId", Models.LogFormatPartType.INTEGER, s => int.Parse(s))
                .AddPart("callingMethod", Models.LogFormatPartType.TEXT, _stubParser)
                .AddPart("message", Models.LogFormatPartType.TEXT, _stubParser)
                .Build();

            return new DelimetedLogFormat('|', partSet, false);
        }

        private static DelimetedLogFormat CreateOutputLogFormat()
        {
            var partSet = new LogFormatPartSetBuilder()
                .AddPart("date", LogFormatPartType.DATETIME,
                    parser: _stubParser,
                    formatter: d => ((DateTime)d).ToString("dd-MM-yyyy", CultureInfo.InvariantCulture))
                .AddPart("time", LogFormatPartType.DATETIME,
                    parser: _stubParser,
                    formatter: d => ((DateTime)d).ToString($"HH:mm:ss.FFFF", CultureInfo.InvariantCulture))
                .AddPart("level", LogFormatPartType.OTHER,
                    parser: _stubParser,
                    formatter: l => LogLevelUtils.EnumToString((LogLevel)l))
                .AddPart("callingMethod", LogFormatPartType.TEXT,
                    parser: _stubParser,
                    formatter: m => m == null ? "DEFAULT" : m.ToString())
                .AddPart("message", LogFormatPartType.TEXT,
                    parser: _stubParser,
                    formatter: msg => msg == null ? String.Empty : msg.ToString())
                .Build();

            return new DelimetedLogFormat('\t', partSet, supportQuotedFields: false);
        }
    }
}
