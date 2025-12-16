using Cleverence.LogTransform.Formats;
using Cleverence.LogTransform.Models;
using Cleverence.LogTransform.Tests.Helpers;
using System.Collections.ObjectModel;

namespace Cleverence.LogTransform.Tests.UnitTests.Models
{
    [TestFixture]
    public class LogEntryUnitTest
    {
        #region Constructor

        [Test]
        public void Constructor_ShouldThrowArgumentNullException_WhenFormatIsNull()
        {
            var values = new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?>());

            var ex = Assert.Throws<ArgumentNullException>(() => new LogEntry(null!, values));
            Assert.That(ex.ParamName, Is.EqualTo("format"));
        }

        [Test]
        public void Constructor_ShouldThrowArgumentNullException_WhenValuesIsNull()
        {
            var format = new MockLogFormat(new[] { "a" }, new Dictionary<string, ILogFormatPart> { ["a"] = new MockLogFormatPart() });

            var ex = Assert.Throws<ArgumentNullException>(() => new LogEntry(format, null!));
            Assert.That(ex.ParamName, Is.EqualTo("values"));
        }

        [Test]
        public void Constructor_ShouldThrowArgumentException_WhenFormatPartCountDoesNotMatchValuesCount()
        {
            var format = new MockLogFormat(
                new[] { "a", "b" },
                new Dictionary<string, ILogFormatPart>
                {
                    ["a"] = new MockLogFormatPart(),
                    ["b"] = new MockLogFormatPart()
                }
            );
            var values = new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?> { ["a"] = "x" });

            var ex = Assert.Throws<ArgumentException>(() => new LogEntry(format, values));
            Assert.That(ex.ParamName, Is.EqualTo("values"));
            Assert.That(ex.Message, Does.Contain("The count of format parts (2) does not match the count of values (1)."));
        }

        [Test]
        public void Constructor_ShouldInitializeProperties_WhenInputsAreValid()
        {
            var part = new MockLogFormatPart();
            var format = new MockLogFormat(new[] { "msg" }, new Dictionary<string, ILogFormatPart> { ["msg"] = part });
            var values = new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?> { ["msg"] = "Hello" });

            var entry = new LogEntry(format, values);

            Assert.That(entry.Format, Is.SameAs(format));
            Assert.That(entry.Values, Is.SameAs(values));
        }

        #endregion

        #region GetValue

        [Test]
        public void GetValue_ShouldReturnCastedValue_WhenKeyExistsAndTypeMatches()
        {
            var format = new MockLogFormat(new[] { "id" }, new Dictionary<string, ILogFormatPart> { ["id"] = new MockLogFormatPart() });
            var values = new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?> { ["id"] = 123 });
            var entry = new LogEntry(format, values);

            var result = entry.GetValue<int>("id");

            Assert.That(result, Is.EqualTo(123));
        }

        [Test]
        public void GetValue_ShouldThrowKeyNotFoundException_WhenKeyDoesNotExist()
        {
            var format = new MockLogFormat(new[] { "msg" }, new Dictionary<string, ILogFormatPart> { ["msg"] = new MockLogFormatPart() });
            var values = new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?> { ["msg"] = "text" });
            var entry = new LogEntry(format, values);

            var ex = Assert.Throws<KeyNotFoundException>(() => entry.GetValue<int>("missing"));
            Assert.That(ex.Message, Does.Contain("Log entry does not contain a value for part 'missing'."));
        }

        [Test]
        public void GetValue_ShouldThrowInvalidCastException_WhenTypeConversionFails()
        {
            var format = new MockLogFormat(new[] { "flag" }, new Dictionary<string, ILogFormatPart> { ["flag"] = new MockLogFormatPart() });
            var values = new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?> { ["flag"] = "not-an-int" });
            var entry = new LogEntry(format, values);

            Assert.Throws<InvalidCastException>(() => entry.GetValue<int>("flag"));
        }

        #endregion

        #region GetRawValue

        [Test]
        public void GetRawValue_ShouldReturnRawValue_WhenKeyExists()
        {
            var format = new MockLogFormat(new[] { "data" }, new Dictionary<string, ILogFormatPart> { ["data"] = new MockLogFormatPart() });
            var values = new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?> { ["data"] = new object() });
            var entry = new LogEntry(format, values);

            var result = entry.GetRawValue("data");

            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void GetRawValue_ShouldThrowKeyNotFoundException_WhenKeyDoesNotExist()
        {
            var format = new MockLogFormat(new[] { "x" }, new Dictionary<string, ILogFormatPart> { ["x"] = new MockLogFormatPart() });
            var values = new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?> { ["x"] = 1 });
            var entry = new LogEntry(format, values);

            var ex = Assert.Throws<KeyNotFoundException>(() => entry.GetRawValue("y"));
            Assert.That(ex.Message, Does.Contain("Log entry does not contain a value for part 'y'."));
        }

        #endregion

        #region GetStringValue

        [Test]
        public void GetStringValue_ShouldReturnStringRepresentation_WhenPartExistsAndIsTextType()
        {
            var part = new MockLogFormatPart { TypeOverride = LogFormatPartType.TEXT };
            var format = new MockLogFormat(new[] { "message" }, new Dictionary<string, ILogFormatPart> { ["message"] = part });
            var values = new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?> { ["message"] = "Hello" });
            var entry = new LogEntry(format, values);

            var result = entry.GetStringValue("message");

            Assert.That(result, Is.EqualTo("Hello"));
        }

        [Test]
        public void GetStringValue_ShouldReturnEmptyString_WhenValueIsNullAndPartIsTextType()
        {
            var part = new MockLogFormatPart { TypeOverride = LogFormatPartType.TEXT };
            var format = new MockLogFormat(new[] { "msg" }, new Dictionary<string, ILogFormatPart> { ["msg"] = part });
            var values = new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?> { ["msg"] = null });
            var entry = new LogEntry(format, values);

            var result = entry.GetStringValue("msg");

            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        public void GetStringValue_ShouldThrowKeyNotFoundException_WhenPartNameDoesNotExist()
        {
            var part = new MockLogFormatPart { TypeOverride = LogFormatPartType.TEXT };
            var format = new MockLogFormat(new[] { "text" }, new Dictionary<string, ILogFormatPart> { ["text"] = part });
            var values = new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?> { ["text"] = "ok" });
            var entry = new LogEntry(format, values);

            var ex = Assert.Throws<KeyNotFoundException>(() => entry.GetStringValue("missing"));
            Assert.That(ex.Message, Does.Contain("Log format does not contain a part named 'missing'."));
        }

        [Test]
        public void GetStringValue_ShouldThrowInvalidOperationException_WhenPartTypeIsNotText()
        {
            var part = new MockLogFormatPart { TypeOverride = LogFormatPartType.INTEGER };
            var format = new MockLogFormat(new[] { "count" }, new Dictionary<string, ILogFormatPart> { ["count"] = part });
            var values = new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?> { ["count"] = 42 });
            var entry = new LogEntry(format, values);

            var ex = Assert.Throws<InvalidOperationException>(() => entry.GetStringValue("count"));
            Assert.That(ex.Message, Does.Contain("Part 'count' is of type INTEGER, but TEXT was expected."));
        }

        #endregion

        #region GetIntegerValue

        [Test]
        public void GetIntegerValue_ShouldReturnInt_WhenPartExistsAndIsIntegerType()
        {
            var part = new MockLogFormatPart { TypeOverride = LogFormatPartType.INTEGER };
            var format = new MockLogFormat(new[] { "id" }, new Dictionary<string, ILogFormatPart> { ["id"] = part });
            var values = new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?> { ["id"] = 100 });
            var entry = new LogEntry(format, values);

            var result = entry.GetIntegerValue("id");

            Assert.That(result, Is.EqualTo(100));
        }

        [Test]
        public void GetIntegerValue_ShouldThrowKeyNotFoundException_WhenPartNameDoesNotExist()
        {
            var part = new MockLogFormatPart { TypeOverride = LogFormatPartType.INTEGER };
            var format = new MockLogFormat(new[] { "num" }, new Dictionary<string, ILogFormatPart> { ["num"] = part });
            var values = new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?> { ["num"] = 1 });
            var entry = new LogEntry(format, values);

            var ex = Assert.Throws<KeyNotFoundException>(() => entry.GetIntegerValue("absent"));
            Assert.That(ex.Message, Does.Contain("Log format does not contain a part named 'absent'."));
        }

        [Test]
        public void GetIntegerValue_ShouldThrowInvalidOperationException_WhenPartTypeIsNotInteger()
        {
            var part = new MockLogFormatPart { TypeOverride = LogFormatPartType.TEXT };
            var format = new MockLogFormat(new[] { "label" }, new Dictionary<string, ILogFormatPart> { ["label"] = part });
            var values = new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?> { ["label"] = "x" });
            var entry = new LogEntry(format, values);

            var ex = Assert.Throws<InvalidOperationException>(() => entry.GetIntegerValue("label"));
            Assert.That(ex.Message, Does.Contain("Part 'label' is of type TEXT, but INTEGER was expected."));
        }

        #endregion

        #region GetFloatValue

        [Test]
        public void GetFloatValue_ShouldReturnFloat_WhenPartExistsAndIsFloatType()
        {
            var part = new MockLogFormatPart { TypeOverride = LogFormatPartType.FLOAT };
            var format = new MockLogFormat(new[] { "ratio" }, new Dictionary<string, ILogFormatPart> { ["ratio"] = part });
            var values = new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?> { ["ratio"] = 3.14f });
            var entry = new LogEntry(format, values);

            var result = entry.GetFloatValue("ratio");

            Assert.That(result, Is.EqualTo(3.14f));
        }

        [Test]
        public void GetFloatValue_ShouldThrowInvalidOperationException_WhenPartTypeIsNotFloat()
        {
            var part = new MockLogFormatPart { TypeOverride = LogFormatPartType.INTEGER };
            var format = new MockLogFormat(new[] { "count" }, new Dictionary<string, ILogFormatPart> { ["count"] = part });
            var values = new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?> { ["count"] = 5 });
            var entry = new LogEntry(format, values);

            var ex = Assert.Throws<InvalidOperationException>(() => entry.GetFloatValue("count"));
            Assert.That(ex.Message, Does.Contain("Part 'count' is of type INTEGER, but FLOAT was expected."));
        }

        #endregion

        #region GetDoubleValue

        [Test]
        public void GetDoubleValue_ShouldReturnDouble_WhenPartExistsAndIsFloatType()
        {
            var part = new MockLogFormatPart { TypeOverride = LogFormatPartType.FLOAT };
            var format = new MockLogFormat(new[] { "measure" }, new Dictionary<string, ILogFormatPart> { ["measure"] = part });
            var values = new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?> { ["measure"] = 2.718 });
            var entry = new LogEntry(format, values);

            var result = entry.GetDoubleValue("measure");

            Assert.That(result, Is.EqualTo(2.718));
        }

        [Test]
        public void GetDoubleValue_ShouldThrowInvalidOperationException_WhenPartTypeIsNotFloat()
        {
            var part = new MockLogFormatPart { TypeOverride = LogFormatPartType.TEXT };
            var format = new MockLogFormat(new[] { "name" }, new Dictionary<string, ILogFormatPart> { ["name"] = part });
            var values = new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?> { ["name"] = "test" });
            var entry = new LogEntry(format, values);

            var ex = Assert.Throws<InvalidOperationException>(() => entry.GetDoubleValue("name"));
            Assert.That(ex.Message, Does.Contain("Part 'name' is of type TEXT, but FLOAT was expected."));
        }

        #endregion

        #region GetDateTimeValue

        [Test]
        public void GetDateTimeValue_ShouldReturnDateTime_WhenPartExistsAndIsDateTimeType()
        {
            var part = new MockLogFormatPart { TypeOverride = LogFormatPartType.DATETIME };
            var format = new MockLogFormat(new[] { "timestamp" }, new Dictionary<string, ILogFormatPart> { ["timestamp"] = part });
            var now = DateTime.UtcNow;
            var values = new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?> { ["timestamp"] = now });
            var entry = new LogEntry(format, values);

            var result = entry.GetDateTimeValue("timestamp");

            Assert.That(result, Is.EqualTo(now));
        }

        [Test]
        public void GetDateTimeValue_ShouldThrowInvalidOperationException_WhenPartTypeIsNotDateTime()
        {
            var part = new MockLogFormatPart { TypeOverride = LogFormatPartType.INTEGER };
            var format = new MockLogFormat(new[] { "epoch" }, new Dictionary<string, ILogFormatPart> { ["epoch"] = part });
            var values = new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?> { ["epoch"] = 123456 });
            var entry = new LogEntry(format, values);

            var ex = Assert.Throws<InvalidOperationException>(() => entry.GetDateTimeValue("epoch"));
            Assert.That(ex.Message, Does.Contain("Part 'epoch' is of type INTEGER, but DATETIME was expected."));
        }

        #endregion
    }
}
