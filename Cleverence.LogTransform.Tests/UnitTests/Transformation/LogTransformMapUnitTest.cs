using Cleverence.LogTransform.Formats;
using Cleverence.LogTransform.Tests.Helpers;
using Cleverence.LogTransform.Transformation;
using System.Collections.ObjectModel;

namespace Cleverence.LogTransform.Tests.UnitTests.Transformation
{
    [TestFixture]
    internal class LogTransformMapUnitTest
    {
        #region Constructor

        [Test]
        public void Constructor_ShouldThrowArgumentNullException_WhenOutputFormatIsNull()
        {
            var transformers = new ReadOnlyDictionary<ILogFormat, FormatBoundLogTransformer>(
                new Dictionary<ILogFormat, FormatBoundLogTransformer>());

            var ex = Assert.Throws<ArgumentNullException>(() =>
                new LogTransformMap(null!, transformers));
            Assert.That(ex.ParamName, Is.EqualTo("outputFormat"));
        }

        [Test]
        public void Constructor_ShouldThrowArgumentNullException_WhenTransformersIsNull()
        {
            var outputFormat = new MockLogFormat([], new Dictionary<string, ILogFormatPart>());

            var ex = Assert.Throws<ArgumentNullException>(() =>
                new LogTransformMap(outputFormat, null!));
            Assert.That(ex.ParamName, Is.EqualTo("transformers"));
        }

        [Test]
        public void Constructor_ShouldThrowArgumentException_WhenTransformersIsEmpty()
        {
            var outputFormat = new MockLogFormat([], new Dictionary<string, ILogFormatPart>());
            var transformers = new ReadOnlyDictionary<ILogFormat, FormatBoundLogTransformer>(
                new Dictionary<ILogFormat, FormatBoundLogTransformer>());

            var ex = Assert.Throws<ArgumentException>(() =>
                new LogTransformMap(outputFormat, transformers));
            Assert.That(ex.ParamName, Is.EqualTo("transformers"));
            Assert.That(ex.Message, Does.Contain("At least one transformation must be registered."));
        }

        [Test]
        public void Constructor_ShouldInitializeProperties_WhenValidArgumentsProvided()
        {
            var outputFormat = new MockLogFormat(new[] { "out" }, new Dictionary<string, ILogFormatPart> { ["out"] = new MockLogFormatPart() });
            var inputFormat = new MockLogFormat(new[] { "in" }, new Dictionary<string, ILogFormatPart> { ["in"] = new MockLogFormatPart() });
            var transformer = FormatBoundLogTransformerFactory.CreateTransformer(outputFormat, inputFormat);
            var transformers = new ReadOnlyDictionary<ILogFormat, FormatBoundLogTransformer>(
                new Dictionary<ILogFormat, FormatBoundLogTransformer> { [inputFormat] = transformer });

            var map = new LogTransformMap(outputFormat, transformers);

            Assert.That(map.OutputFormat, Is.SameAs(outputFormat));
            Assert.That(map.InputFormats.ToArray(), Has.Length.EqualTo(1));
            Assert.That(map.InputFormats, Contains.Item(inputFormat));
        }

        #endregion

        #region TryGetTransformer

        [Test]
        public void TryGetTransformer_ShouldReturnTrueAndTransformer_WhenInputFormatExists()
        {
            var outputFormat = new MockLogFormat([], new Dictionary<string, ILogFormatPart>());
            var inputFormat = new MockLogFormat([], new Dictionary<string, ILogFormatPart>());
            var transformer = FormatBoundLogTransformerFactory.CreateTransformer(outputFormat, inputFormat);
            var transformers = new ReadOnlyDictionary<ILogFormat, FormatBoundLogTransformer>(
                new Dictionary<ILogFormat, FormatBoundLogTransformer> { [inputFormat] = transformer });
            var map = new LogTransformMap(outputFormat, transformers);

            var success = map.TryGetTransformer(inputFormat, out var result);

            Assert.That(success, Is.True);
            Assert.That(result, Is.SameAs(transformer));
        }

        [Test]
        public void TryGetTransformer_ShouldReturnFalseAndNull_WhenInputFormatDoesNotExist()
        {
            var outputFormat = new MockLogFormat([], new Dictionary<string, ILogFormatPart>());
            var inputFormat = new MockLogFormat([], new Dictionary<string, ILogFormatPart>());
            var anotherFormat = new MockLogFormat([], new Dictionary<string, ILogFormatPart>());
            var transformer = FormatBoundLogTransformerFactory.CreateTransformer(outputFormat, inputFormat);
            var transformers = new ReadOnlyDictionary<ILogFormat, FormatBoundLogTransformer>(
                new Dictionary<ILogFormat, FormatBoundLogTransformer> { [inputFormat] = transformer });
            var map = new LogTransformMap(outputFormat, transformers);

            var success = map.TryGetTransformer(anotherFormat, out var result);

            Assert.That(success, Is.False);
            Assert.That(result, Is.Null);
        }

        #endregion

        #region GetTransformer

        [Test]
        public void GetTransformer_ShouldReturnTransformer_WhenInputFormatExists()
        {
            var outputFormat = new MockLogFormat([], new Dictionary<string, ILogFormatPart>());
            var inputFormat = new MockLogFormat([], new Dictionary<string, ILogFormatPart>());
            var transformer = FormatBoundLogTransformerFactory.CreateTransformer(outputFormat, inputFormat);
            var transformers = new ReadOnlyDictionary<ILogFormat, FormatBoundLogTransformer>(
                new Dictionary<ILogFormat, FormatBoundLogTransformer> { [inputFormat] = transformer });
            var map = new LogTransformMap(outputFormat, transformers);

            var result = map.GetTransformer(inputFormat);

            Assert.That(result, Is.SameAs(transformer));
        }

        [Test]
        public void GetTransformer_ShouldThrowKeyNotFoundException_WhenInputFormatDoesNotExist()
        {
            var outputFormat = new MockLogFormat([], new Dictionary<string, ILogFormatPart>());
            var inputFormat = new MockLogFormat([], new Dictionary<string, ILogFormatPart>());
            var missingFormat = new MockLogFormat([], new Dictionary<string, ILogFormatPart>());
            var transformer = FormatBoundLogTransformerFactory.CreateTransformer(outputFormat, inputFormat);
            var transformers = new ReadOnlyDictionary<ILogFormat, FormatBoundLogTransformer>(
                new Dictionary<ILogFormat, FormatBoundLogTransformer> { [inputFormat] = transformer });
            var map = new LogTransformMap(outputFormat, transformers);

            var ex = Assert.Throws<KeyNotFoundException>(() => map.GetTransformer(missingFormat));
            Assert.That(ex.Message, Does.Contain("No transformation registered for the specified input log format."));
        }

        #endregion

        #region InputFormats

        [Test]
        public void InputFormats_ShouldReturnAllRegisteredInputFormats()
        {
            var outputFormat = new MockLogFormat([], new Dictionary<string, ILogFormatPart>());
            var input1 = new MockLogFormat([], new Dictionary<string, ILogFormatPart>());
            var input2 = new MockLogFormat([], new Dictionary<string, ILogFormatPart>());
            var t1 = FormatBoundLogTransformerFactory.CreateTransformer(outputFormat, input1);
            var t2 = FormatBoundLogTransformerFactory.CreateTransformer(outputFormat, input2);
            var transformers = new ReadOnlyDictionary<ILogFormat, FormatBoundLogTransformer>(
                new Dictionary<ILogFormat, FormatBoundLogTransformer>
                {
                    [input1] = t1,
                    [input2] = t2
                });
            var map = new LogTransformMap(outputFormat, transformers);

            var inputs = new List<ILogFormat>(map.InputFormats);

            Assert.That(inputs, Has.Count.EqualTo(2));
            Assert.That(inputs, Contains.Item(input1));
            Assert.That(inputs, Contains.Item(input2));
        }

        [Test]
        public void InputFormats_ShouldReturnReadOnlyCollection()
        {
            var outputFormat = new MockLogFormat([], new Dictionary<string, ILogFormatPart>());
            var inputFormat = new MockLogFormat([], new Dictionary<string, ILogFormatPart>());
            var transformer = FormatBoundLogTransformerFactory.CreateTransformer(outputFormat, inputFormat);
            var transformers = new ReadOnlyDictionary<ILogFormat, FormatBoundLogTransformer>(
                new Dictionary<ILogFormat, FormatBoundLogTransformer> { [inputFormat] = transformer });
            var map = new LogTransformMap(outputFormat, transformers);

            Assert.That(map.InputFormats, Is.InstanceOf<IEnumerable<ILogFormat>>());
            Assert.That(map.InputFormats, Is.Not.InstanceOf<IList<ILogFormat>>());
        }

        #endregion
    }
}
