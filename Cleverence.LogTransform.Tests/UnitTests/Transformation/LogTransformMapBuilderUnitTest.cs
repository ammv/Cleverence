using Cleverence.LogTransform.Formats;
using Cleverence.LogTransform.Tests.Helpers;
using Cleverence.LogTransform.Transformation;

namespace Cleverence.LogTransform.Tests.UnitTests.Transformation
{
    [TestFixture]
    internal class LogTransformMapBuilderUnitTest
    {
        #region Constructor

        [Test]
        public void Constructor_ShouldThrowArgumentNullException_WhenOutputFormatIsNull()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new LogTransformMapBuilder(null!));
            Assert.That(ex.ParamName, Is.EqualTo("outputFormat"));
        }

        [Test]
        public void Constructor_ShouldInitializeWithGivenOutputFormat_WhenValid()
        {
            var outputFormat = new MockLogFormat([], new Dictionary<string, ILogFormatPart>());
            var builder = new LogTransformMapBuilder(outputFormat);

            // Проверка через Build ниже; здесь гарантируем, что не падает и принимает формат
            Assert.That(builder, Is.Not.Null);
        }

        #endregion

        #region Add

        [Test]
        public void Add_ShouldThrowArgumentNullException_WhenInputFormatIsNull()
        {
            var outputFormat = new MockLogFormat([], new Dictionary<string, ILogFormatPart>());
            var builder = new LogTransformMapBuilder(outputFormat);
            var transformer = FormatBoundLogTransformerFactory.CreateTransformer(outputFormat);

            var ex = Assert.Throws<ArgumentNullException>(() => builder.Add(null!, transformer));
            Assert.That(ex.ParamName, Is.EqualTo("inputFormat"));
        }

        [Test]
        public void Add_ShouldThrowArgumentNullException_WhenTransformerIsNull()
        {
            var outputFormat = new MockLogFormat([], new Dictionary<string, ILogFormatPart>());
            var builder = new LogTransformMapBuilder(outputFormat);
            var inputFormat = new MockLogFormat([], new Dictionary<string, ILogFormatPart>());

            var ex = Assert.Throws<ArgumentNullException>(() => builder.Add(inputFormat, null!));
            Assert.That(ex.ParamName, Is.EqualTo("transformer"));
        }

        [Test]
        public void Add_ShouldThrowInvalidOperationException_WhenTransformerForInputFormatAlreadyExists()
        {
            var outputFormat = new MockLogFormat([], new Dictionary<string, ILogFormatPart>());
            var inputFormat = new MockLogFormat([], new Dictionary<string, ILogFormatPart>());
            var builder = new LogTransformMapBuilder(outputFormat);
            var t1 = FormatBoundLogTransformerFactory.CreateTransformer(outputFormat);
            var t2 = FormatBoundLogTransformerFactory.CreateTransformer(outputFormat);

            builder.Add(inputFormat, t1);

            var ex = Assert.Throws<InvalidOperationException>(() => builder.Add(inputFormat, t2));
            Assert.That(ex.Message, Does.Contain($"A transformation for input format '{inputFormat}' is already registered."));
        }

        [Test]
        public void Add_ShouldRegisterTransformer_WhenInputFormatAndTransformerAreValidAndNotDuplicate()
        {
            var outputFormat = new MockLogFormat([], new Dictionary<string, ILogFormatPart>());
            var inputFormat = new MockLogFormat([], new Dictionary<string, ILogFormatPart>());
            var builder = new LogTransformMapBuilder(outputFormat);
            var transformer = FormatBoundLogTransformerFactory.CreateTransformer(outputFormat);

            var result = builder.Add(inputFormat, transformer);

            Assert.That(result, Is.SameAs(builder));
            var map = builder.Build();
            Assert.That(map.InputFormats, Contains.Item(inputFormat));
            Assert.That(map.GetTransformer(inputFormat), Is.SameAs(transformer));
        }

        #endregion

        #region Build

        [Test]
        public void Build_ShouldThrowInvalidOperationException_WhenNoTransformersAdded()
        {
            var outputFormat = new MockLogFormat([], new Dictionary<string, ILogFormatPart>());
            var builder = new LogTransformMapBuilder(outputFormat);

            var ex = Assert.Throws<InvalidOperationException>(() => builder.Build());
            Assert.That(ex.Message, Does.Contain("At least one transformation must be added before building the map."));
        }

        [Test]
        public void Build_ShouldReturnLogTransformMap_WhenAtLeastOneTransformerAdded()
        {
            var outputFormat = new MockLogFormat(["out"], new Dictionary<string, ILogFormatPart> { ["out"] = new MockLogFormatPart() });
            var inputFormat = new MockLogFormat(["in"], new Dictionary<string, ILogFormatPart> { ["in"] = new MockLogFormatPart() });
            var builder = new LogTransformMapBuilder(outputFormat);
            var transformer = FormatBoundLogTransformerFactory.CreateTransformer(outputFormat, inputFormat);

            builder.Add(inputFormat, transformer);
            var map = builder.Build();

            Assert.That(map, Is.Not.Null);
            Assert.That(map.OutputFormat, Is.SameAs(outputFormat));
            Assert.That(map.InputFormats.ToArray(), Has.Length.EqualTo(1));
            Assert.That(map.InputFormats, Contains.Item(inputFormat));
            Assert.That(map.GetTransformer(inputFormat), Is.SameAs(transformer));
        }

        [Test]
        public void Build_ShouldReturnImmutableMap_WhenCalledMultipleTimes()
        {
            var outputFormat = new MockLogFormat([], new Dictionary<string, ILogFormatPart>());
            var inputFormat1 = new MockLogFormat(["1"], new Dictionary<string, ILogFormatPart>() { ["1"] = new MockLogFormatPart() });
            var inputFormat2 = new MockLogFormat(["2"], new Dictionary<string, ILogFormatPart>() { ["2"] = new MockLogFormatPart() });
            var builder = new LogTransformMapBuilder(outputFormat);

            builder
                .Add(inputFormat1, FormatBoundLogTransformerFactory.CreateTransformer(outputFormat, inputFormat1))
                .Add(inputFormat2, FormatBoundLogTransformerFactory.CreateTransformer(outputFormat, inputFormat2));

            var map1 = builder.Build();
            var map2 = builder.Build();

            Assert.That(map1.InputFormats.ToArray(), Has.Length.EqualTo(2));
            Assert.That(map2.InputFormats.ToArray(), Has.Length.EqualTo(2));
            CollectionAssert.AreEquivalent(new[] { inputFormat1, inputFormat2 }, map1.InputFormats);
            CollectionAssert.AreEquivalent(new[] { inputFormat1, inputFormat2 }, map2.InputFormats);
        }

        #endregion
    }
}
