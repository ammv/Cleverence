using Cleverence.LogTransform.Formats;
using Cleverence.LogTransform.Models;

namespace Cleverence.LogTransform.Tests;

[TestFixture]
public class LogFormatPartDefaultUnitTest
{
    #region Constructor

    [Test]
    public void Constructor_ShouldThrowArgumentNullException_WhenParserIsNull()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new LogFormatPartDefault(LogFormatPartType.TEXT, null!));
        Assert.That(ex.ParamName, Is.EqualTo("parser"));
    }

    [Test]
    public void Constructor_ShouldSetType_WhenValidTypeProvided()
    {
        var parser = new Func<string, object?>(s => s);
        var part = new LogFormatPartDefault(LogFormatPartType.DATETIME, parser);

        Assert.That(part.Type, Is.EqualTo(LogFormatPartType.DATETIME));
    }

    [Test]
    public void Constructor_ShouldWrapParserIntoTryFunc_WhenParserProvided()
    {
        const string input = "test";
        var parser = new Func<string, object?>(s => s == "test" ? 42 : null);
        var part = new LogFormatPartDefault(LogFormatPartType.INTEGER, parser);

        part.Parser.TryInvoke(input, out var result);
        Assert.That(result, Is.EqualTo(42));
    }

    [Test]
    public void Constructor_ShouldUseDefaultFormatter_WhenFormatterIsNull()
    {
        var parser = new Func<string, object?>(s => s);
        var part = new LogFormatPartDefault(LogFormatPartType.TEXT, parser, null);

        part.Formatter.TryInvoke(123, out var result);
        Assert.That(result, Is.EqualTo("123"));

        part.Formatter.TryInvoke(null, out result);
        Assert.That(result, Is.EqualTo(string.Empty));
    }

    [Test]
    public void Constructor_ShouldWrapCustomFormatterIntoTryFunc_WhenFormatterProvided()
    {
        var parser = new Func<string, object?>(s => s);
        var formatter = new Func<object?, string>(o => $"[{o}]");
        var part = new LogFormatPartDefault(LogFormatPartType.TEXT, parser, formatter);

        part.Formatter.TryInvoke("hello", out var result);
        Assert.That(result, Is.EqualTo("[hello]"));

        part.Formatter.TryInvoke(null, out result);
        Assert.That(result, Is.EqualTo("[]"));
    }

    #endregion

    #region Parser

    [Test]
    public void Parser_ShouldInvokeProvidedParserFunction_WhenCalled()
    {
        var invoked = false;
        object? capturedInput = null;
        var parser = new Func<string, object?>(s =>
        {
            invoked = true;
            capturedInput = s;
            return "parsed";
        });
        var part = new LogFormatPartDefault(LogFormatPartType.TEXT, parser);

        part.Parser.TryInvoke("input", out var result);

        Assert.That(invoked, Is.True, "Parser delegate should have been invoked.");
        Assert.That(capturedInput, Is.EqualTo("input"));
        Assert.That(result, Is.EqualTo("parsed"));
    }

    [Test]
    public void Parser_ShouldReturnNull_WhenParserReturnsNull()
    {
        var parser = new Func<string, object?>(_ => null);
        var part = new LogFormatPartDefault(LogFormatPartType.TEXT, parser);

        part.Parser.TryInvoke("anything", out var result);
        Assert.That(result, Is.Null);
    }

    #endregion

    #region Formatter

    [Test]
    public void Formatter_ShouldUseDefault_WhenCustomFormatterWasNotProvided()
    {
        var parser = new Func<string, object?>(s => s);
        var part = new LogFormatPartDefault(LogFormatPartType.TEXT, parser);

        part.Formatter.TryInvoke("value", out var result);
        Assert.That(result, Is.EqualTo("value"));

        part.Formatter.TryInvoke(null, out result);
        Assert.That(result, Is.EqualTo(string.Empty));
    }

    [Test]
    public void Formatter_ShouldUseCustom_WhenProvided()
    {
        var parser = new Func<string, object?>(s => s);
        var formatter = new Func<object?, string>(o => o?.ToString().ToUpperInvariant() ?? "MISSING");
        var part = new LogFormatPartDefault(LogFormatPartType.TEXT, parser, formatter);

        part.Formatter.TryInvoke("hello", out var result);
        Assert.That(result, Is.EqualTo("HELLO"));

        part.Formatter.TryInvoke(null, out result);
        Assert.That(result, Is.EqualTo("MISSING"));
    }

    #endregion
}
