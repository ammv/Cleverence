using Cleverence.LogTransform.Formats;
using Cleverence.LogTransform.Models;
using Cleverence.LogTransform.Tests.Helpers;
using Cleverence.LogTransform.Transformation;

namespace Cleverence.LogTransform.Tests;

[TestFixture]
public class FormatBoundLogTransformerUnitTest
{
    #region Create

    [Test]
    public void Create_ShouldThrowArgumentNullException_WhenExpectedInputFormatIsNull()
    {
        var transform = new Func<LogEntry, LogEntry>(e => e);

        var ex = Assert.Throws<ArgumentNullException>(() =>
            FormatBoundLogTransformer.Create(null!, transform));
        Assert.That(ex.ParamName, Is.EqualTo("expectedInputFormat"));
    }

    [Test]
    public void Create_ShouldThrowArgumentNullException_WhenTransformIsNull()
    {
        var format = new MockLogFormat([], new Dictionary<string, ILogFormatPart>());

        var ex = Assert.Throws<ArgumentNullException>(() =>
            FormatBoundLogTransformer.Create(format, null!));
        Assert.That(ex.ParamName, Is.EqualTo("transform"));
    }

    [Test]
    public void Create_ShouldReturnNewInstance_WhenArgumentsAreValid()
    {
        var format = new MockLogFormat([], new Dictionary<string, ILogFormatPart>());
        var transform = new Func<LogEntry, LogEntry>(e => e);

        var transformer = FormatBoundLogTransformer.Create(format, transform);

        Assert.That(transformer, Is.Not.Null);
        Assert.That(transformer.ExpectedInputFormat, Is.SameAs(format));
    }

    #endregion

    #region Transform

    [Test]
    public void Transform_ShouldThrowArgumentException_WhenInputFormatDoesNotMatchExpected()
    {
        var expectedFormat = new MockLogFormat([], new Dictionary<string, ILogFormatPart>());
        var actualFormat = new MockLogFormat([], new Dictionary<string, ILogFormatPart>());
        var transform = new Func<LogEntry, LogEntry>(e => e);
        var transformer = FormatBoundLogTransformer.Create(expectedFormat, transform);

        var entry = LogEntryFactory.CreateLogEntry(actualFormat);

        var ex = Assert.Throws<ArgumentException>(() => transformer.Transform(entry));
        Assert.That(ex.Message, Does.Contain($"Input LogEntry has format '{actualFormat}', but expected '{expectedFormat}'."));
    }

    [Test]
    public void Transform_ShouldInvokeTransformFunction_WhenInputFormatMatchesExpected()
    {
        var format = new MockLogFormat([], new Dictionary<string, ILogFormatPart>());
        LogEntry capturedInput = null!;
        var transform = new Func<LogEntry, LogEntry>(e =>
        {
            capturedInput = e;
            return e;
        });
        var transformer = FormatBoundLogTransformer.Create(format, transform);

        var entry = LogEntryFactory.CreateLogEntry(format);
        var result = transformer.Transform(entry);

        Assert.That(capturedInput, Is.SameAs(entry));
        Assert.That(result, Is.SameAs(entry));
    }

    [Test]
    public void Transform_ShouldReturnResultOfTransformFunction_WhenCalled()
    {
        var format = new MockLogFormat([], new Dictionary<string, ILogFormatPart>());
        var originalEntry = LogEntryFactory.CreateLogEntry(format);
        var newEntry = LogEntryFactory.CreateLogEntry(format);
        var transform = new Func<LogEntry, LogEntry>(_ => newEntry);
        var transformer = FormatBoundLogTransformer.Create(format, transform);

        var result = transformer.Transform(originalEntry);

        Assert.That(result, Is.SameAs(newEntry));
    }

    #endregion
}
