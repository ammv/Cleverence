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

namespace Cleverence.LogTransform.Tests.IntegrationTests.EndToEnd
{
    [TestFixture, Category("Integration")]
    public class FullLogPipelineIntegrationTest
    {
        #region EndToEnd_Transformation

        [Test]
        public void EndToEnd_ParseFormat1AndFormat2TransformToUnifiedOutput_ShouldProduceExpectedTabSeparatedLines()
        {
            // === Входной формат 1: "10.03.2025 15:14:49.523 INFORMATION Версия программы: '3.4.0.48729'"

            string[] supportedLogLevels1 = ["INFORMATION", "WARNING", "DEBUG", "ERROR"];
            string[] supportedLogLevels2 = ["INFO", "WARN", "DEBUG", "ERROR"];

            var inputFormat1PartSet = new LogFormatPartSetBuilder()
                .AddPart("date", LogFormatPartType.DATETIME, s => DateTime.ParseExact(s, "dd.MM.yyyy", CultureInfo.InvariantCulture))
                .AddPart("time", LogFormatPartType.DATETIME, s => DateTime.ParseExact(s, "HH:mm:ss.fff", CultureInfo.InvariantCulture))
                .AddPart("level", LogFormatPartType.TEXT, s =>
                { 
                    if(!supportedLogLevels1.Contains(s))
                    {
                        throw new ArgumentException("Unsupported log level provided");
                    }
                    return s.Trim();
                })
                .AddPart("message", LogFormatPartType.TEXT, s => s)
                .Build();

            var inputFormat1 = new DelimetedLogFormat(
                separator: ' ',
                inputFormat1PartSet,
                supportQuotedFields: false);

            // === Входной формат 2: "2025-03-10 15:14:51.5882| INFO|11|MobileComputer.GetDeviceId| Код устройства: ..."

            var inputFormat2PartSet = new LogFormatPartSetBuilder()
                .AddPart("datetime", LogFormatPartType.DATETIME, s => DateTime.ParseExact(s, "yyyy-MM-dd HH:mm:ss.ffff", CultureInfo.InvariantCulture))
                .AddPart("level", LogFormatPartType.TEXT, s =>
                {
                    s = s.Trim();
                    if (!supportedLogLevels2.Contains(s))
                    {
                        throw new ArgumentException("Unsupported log level provided");
                    }
                    return s;
                })
                .AddPart("thread", LogFormatPartType.INTEGER, s => int.Parse(s))
                .AddPart("method", LogFormatPartType.TEXT, s => s.Trim())
                .AddPart("message", LogFormatPartType.TEXT, s => s.TrimStart())
                .Build();

            var inputFormat2 = new DelimetedLogFormat(
                separator: '|',
                inputFormat2PartSet,
                supportQuotedFields: false);

            // === Выходной формат: "DD-MM-YYYY<TAB>time<TAB>level<TAB>method<TAB>message"
            var outputPartSet = new LogFormatPartSetBuilder()
                .AddPart("date", LogFormatPartType.DATETIME,
                    parser: s => s,
                    formatter: d => ((DateTime)d).ToString("dd-MM-yyyy", CultureInfo.InvariantCulture))
                .AddPart("time", LogFormatPartType.TEXT,
                    parser: s => s,
                    formatter: d => ((DateTime)d).ToString($"HH:mm:ss.FFFF"))
                .AddPart("level", LogFormatPartType.TEXT,
                    parser: s => s,
                    formatter: l => l?.ToString())
                .AddPart("callingMethod", LogFormatPartType.TEXT,
                    parser: s => s,
                    formatter: m => m == null ? "DEFAULT" : m.ToString())
                .AddPart("message", LogFormatPartType.TEXT,
                    parser: s => s,
                    formatter: msg => msg == null ? String.Empty : msg.ToString())
                .Build();

            var outputFormat = new DelimetedLogFormat('\t', outputPartSet, supportQuotedFields: false);

            // === Функция нормализации уровня
            string NormalizeLevel(string inputLevel)
            {
                return inputLevel switch
                {
                    "INFORMATION" => "INFO",
                    "WARNING" => "WARN",
                    _ => inputLevel
                };
            }

            // === Преобразователь для формата 1
            var transform1 = new Func<LogEntry, LogEntry>(entry =>
            {
                var values = new Dictionary<string, object?>
                {
                    ["date"] = entry.GetDateTimeValue("date"),
                    ["time"] = entry.GetDateTimeValue("time"),
                    ["level"] = NormalizeLevel(entry.GetStringValue("level")),
                    ["callingMethod"] = null, // будет "DEFAULT"
                    ["message"] = entry.GetStringValue("message")
                };
                return new LogEntry(outputFormat, new ReadOnlyDictionary<string, object?>(values));
            });

            // === Преобразователь для формата 2
            var transform2 = new Func<LogEntry, LogEntry>(entry =>
            {
                var values = new Dictionary<string, object?>
                {
                    ["date"] = entry.GetDateTimeValue("datetime"),
                    ["time"] = entry.GetDateTimeValue("datetime"),
                    ["level"] = NormalizeLevel(entry.GetStringValue("level")),
                    ["callingMethod"] = entry.GetStringValue("method"),
                    ["message"] = entry.GetStringValue("message")
                };
                return new LogEntry(outputFormat, new ReadOnlyDictionary<string, object?>(values));
            });

            // === Сборка карты преобразований
            var transformer1 = FormatBoundLogTransformer.Create(inputFormat1, transform1);
            var transformer2 = FormatBoundLogTransformer.Create(inputFormat2, transform2);

            var transformMap = new LogTransformMapBuilder(outputFormat)
                .Add(inputFormat1, transformer1)
                .Add(inputFormat2, transformer2)
                .Build();

            // === Парсер, поддерживающий оба входных формата
            var parser = new LogParser(new[] { inputFormat1, inputFormat2 });
            var logTransformer = new LogTransformer(transformMap);
            var formatter = new LogFormatter();

            // === Тест 1: Формат 1 → выход
            const string inputLine1 = "10.03.2025 15:14:49.523 INFORMATION Версия программы: '3.4.0.48729'";
            var entry1 = parser.Parse(inputLine1);
            var transformed1 = logTransformer.Transform(entry1);
            var output1 = formatter.Format(transformed1);

            // === Тест 2: Формат 2 → выход
            const string inputLine2 = "2025-03-10 15:14:51.5882| INFO|11|MobileComputer.GetDeviceId| Код устройства: '@MINDEO-M40-D-410244015546'";
            var entry2 = parser.Parse(inputLine2);
            var transformed2 = logTransformer.Transform(entry2);
            var output2 = formatter.Format(transformed2);

            // === Проверки
            const string expected1 = "10-03-2025\t15:14:49.523\tINFO\tDEFAULT\tВерсия программы: '3.4.0.48729'";
            const string expected2 = "10-03-2025\t15:14:51.5882\tINFO\tMobileComputer.GetDeviceId\tКод устройства: '@MINDEO-M40-D-410244015546'";

            Assert.That(output1, Is.EqualTo(expected1));
            Assert.That(output2, Is.EqualTo(expected2));
        }

        #endregion
    }
}
