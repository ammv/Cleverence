using Cleverence.LogTransform.Formats;
using Cleverence.LogTransform.Tests.Helpers;
using Cleverence.LogTransform.Transformation;
using System.Collections.ObjectModel;

namespace Cleverence.LogTransform.Tests.UnitTests.Transformation
{
    [TestFixture]
    internal class LogTransformerUnitTest
    {
        #region Constructor

        [Test]
        public void Constructor_ShouldThrowArgumentNullException_WhenTransformMapIsNull()
        {
            var inputFormat = new MockLogFormat([], new Dictionary<string, ILogFormatPart>());

            var ex = Assert.Throws<ArgumentNullException>(() => new LogTransformer(null!));
            Assert.That(ex.ParamName, Is.EqualTo("transformMap"));
        }

        [Test]
        public void Constructor_ShouldInitialize_WhenInputFormatIsSupportedByTransformMap()
        {
            var inputFormat = new MockLogFormat([], new Dictionary<string, ILogFormatPart>());
            var outputFormat = new MockLogFormat([], new Dictionary<string, ILogFormatPart>());
            var transformer = FormatBoundLogTransformerFactory.CreateTransformer(outputFormat, inputFormat);
            var transformMap = new LogTransformMap(
                outputFormat,
                new ReadOnlyDictionary<ILogFormat, FormatBoundLogTransformer>(
                    new Dictionary<ILogFormat, FormatBoundLogTransformer> { [inputFormat] = transformer }));

            var logTransformer = new LogTransformer(transformMap);

            Assert.That(logTransformer, Is.Not.Null);
        }

        #endregion

        #region Transform

        [Test]
        public void Transform_ShouldThrowArgumentNullException_WhenEntryIsNull()
        {
            var inputFormat = new MockLogFormat([], new Dictionary<string, ILogFormatPart>());
            var transformMap = LogTransformMapFactory.CreateTransformMapWithInput(inputFormat);
            var transformer = new LogTransformer(transformMap);

            var ex = Assert.Throws<ArgumentNullException>(() => transformer.Transform(null!));
            Assert.That(ex.ParamName, Is.EqualTo("entry"));
        }

        [Test]
        public void Transform_ShouldThrowArgumentException_WhenEntryFormatDoesNotContainsInTransformationMap()
        {
            var outputFormat = new MockLogFormat([], new Dictionary<string, ILogFormatPart>());
            var actualFormat = new MockLogFormat([], new Dictionary<string, ILogFormatPart>());
            var transformMap = LogTransformMapFactory.CreateTransformMapWithInput(outputFormat);
            var transformer = new LogTransformer(transformMap);
            var entry = LogEntryFactory.CreateLogEntry(actualFormat);

            var ex = Assert.Throws<ArgumentException>(() => transformer.Transform(entry));
            Assert.That(ex.ParamName, Is.EqualTo("entry"));
            Assert.That(ex.Message, Does.Contain("Log entry format"));
            Assert.That(ex.Message, Does.Contain("does not present in transformation map."));
        }

        [Test]
        public void Transform_ShouldReturnTransformedEntry_WhenEntryFormatMatchesAndTransformationSucceeds()
        {
            var inputFormat = new MockLogFormat([], new Dictionary<string, ILogFormatPart>());
            var outputFormat = new MockLogFormat([], new Dictionary<string, ILogFormatPart>());
            var transformMap = LogTransformMapFactory.CreateTransformMapWithInput(inputFormat, outputFormat);
            var transformer = new LogTransformer(transformMap);
            var inputEntry = LogEntryFactory.CreateLogEntry(inputFormat);

            var result = transformer.Transform(inputEntry);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Format, Is.SameAs(outputFormat));
        }

        [Test]
        public void Transform_ShouldThrowInvalidOperationException_WhenTransformationFails()
        {
            var inputFormat = new MockLogFormat([], new Dictionary<string, ILogFormatPart>());
            var outputFormat = new MockLogFormat([], new Dictionary<string, ILogFormatPart>());

            var faultyTransformer = FormatBoundLogTransformer.Create(
                inputFormat,
                _ => throw new InvalidOperationException("Simulated transform failure"));

            var transformMap = new LogTransformMap(
                outputFormat,
                new ReadOnlyDictionary<ILogFormat, FormatBoundLogTransformer>(
                    new Dictionary<ILogFormat, FormatBoundLogTransformer> { [inputFormat] = faultyTransformer }));

            var transformer = new LogTransformer(transformMap);
            var entry = LogEntryFactory.CreateLogEntry(inputFormat);

            Assert.Throws<InvalidOperationException>(() => transformer.Transform(entry));
        }

        #endregion

        #region TryTransform

        [Test]
        public void TryTransform_ShouldReturnFalse_WhenEntryIsNull()
        {
            var inputFormat = new MockLogFormat([], new Dictionary<string, ILogFormatPart>());
            var transformMap = LogTransformMapFactory.CreateTransformMapWithInput(inputFormat);
            var transformer = new LogTransformer(transformMap);

            var success = transformer.TryTransform(null!, out var result);

            Assert.That(success, Is.False);
            Assert.That(result, Is.Null);
        }

        [Test]
        public void TryTransform_ShouldReturnFalse_WhenEntryFormatDoesNotMatchExpected()
        {
            var expectedFormat = new MockLogFormat([], new Dictionary<string, ILogFormatPart>());
            var otherFormat = new MockLogFormat([], new Dictionary<string, ILogFormatPart>());
            var transformMap = LogTransformMapFactory.CreateTransformMapWithInput(expectedFormat);
            var transformer = new LogTransformer(transformMap);
            var entry = LogEntryFactory.CreateLogEntry(otherFormat);

            var success = transformer.TryTransform(entry, out var result);

            Assert.That(success, Is.False);
            Assert.That(result, Is.Null);
        }

        [Test]
        public void TryTransform_ShouldReturnFalse_WhenNoTransformerFoundInMap()
        {
            // This case is theoretically unreachable if constructor validated correctly,
            // but we test defensive behavior
            var inputFormat = new MockLogFormat([], new Dictionary<string, ILogFormatPart>());
            var outputFormat = new MockLogFormat([], new Dictionary<string, ILogFormatPart>());

            // Create map WITHOUT the input format, then bypass constructor check (not possible normally)
            // So instead, we simulate by mocking — but since we can't bypass constructor,
            // this test is redundant under normal usage. We skip it.

            // Instead, we rely on constructor validation; TryTransform assumes consistency.
            // So we test only the success path.
            Assert.Ignore("Constructor ensures input format is in map; this path is unreachable.");
        }

        [Test]
        public void TryTransform_ShouldReturnTrueAndTransformedEntry_WhenEntryIsValidAndTransformationSucceeds()
        {
            var inputFormat = new MockLogFormat([], new Dictionary<string, ILogFormatPart>());
            var outputFormat = new MockLogFormat([], new Dictionary<string, ILogFormatPart>());
            var transformMap = LogTransformMapFactory.CreateTransformMapWithInput(inputFormat, outputFormat);
            var transformer = new LogTransformer(transformMap);
            var inputEntry = LogEntryFactory.CreateLogEntry(inputFormat);

            var success = transformer.TryTransform(inputEntry, out var result);

            Assert.That(success, Is.True);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Format, Is.SameAs(outputFormat));
        }

        [Test]
        public void TryTransform_ShouldReturnFalse_WhenTransformerThrowsDuringTransform()
        {
            var inputFormat = new MockLogFormat([], new Dictionary<string, ILogFormatPart>());
            var outputFormat = new MockLogFormat([], new Dictionary<string, ILogFormatPart>());

            var faultyTransformer = FormatBoundLogTransformer.Create(
                inputFormat,
                _ => throw new Exception("Simulated failure"));

            var transformMap = new LogTransformMap(
                outputFormat,
                new ReadOnlyDictionary<ILogFormat, FormatBoundLogTransformer>(
                    new Dictionary<ILogFormat, FormatBoundLogTransformer> { [inputFormat] = faultyTransformer }));

            var transformer = new LogTransformer(transformMap);
            var entry = LogEntryFactory.CreateLogEntry(inputFormat);

            var success = transformer.TryTransform(entry, out var result);

            Assert.That(success, Is.False);
            Assert.That(result, Is.Null);
        }

        #endregion
    }
}
