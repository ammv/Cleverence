using Cleverence.LogTransform.Formats.Builders;
using Cleverence.LogTransform.Models;
using Cleverence.LogTransform.Tests.Helpers;

namespace Cleverence.LogTransform.Tests;

[TestFixture]
public class LogFormatPartSetBuilderUnitTest
{
    #region AddPart_String_ILogFormatPart

    [Test]
    public void AddPart_String_ILogFormatPart_ShouldThrowArgumentNullException_WhenNameIsNull()
    {
        var builder = new LogFormatPartSetBuilder();
        var part = new MockLogFormatPart();

        var ex = Assert.Throws<ArgumentNullException>(() => builder.AddPart(null!, part));
        Assert.That(ex.ParamName, Is.EqualTo("name"));
    }

    [Test]
    public void AddPart_String_ILogFormatPart_ShouldThrowArgumentNullException_WhenPartIsNull()
    {
        var builder = new LogFormatPartSetBuilder();

        var ex = Assert.Throws<ArgumentNullException>(() => builder.AddPart("name", null!));
        Assert.That(ex.ParamName, Is.EqualTo("part"));
    }

    [Test]
    public void AddPart_String_ILogFormatPart_ShouldThrowInvalidOperationException_WhenPartWithNameAlreadyExists()
    {
        var builder = new LogFormatPartSetBuilder();
        var part1 = new MockLogFormatPart();
        var part2 = new MockLogFormatPart();

        builder.AddPart("duplicate", part1);

        var ex = Assert.Throws<InvalidOperationException>(() => builder.AddPart("duplicate", part2));
        Assert.That(ex.Message, Does.Contain("A log part with name 'duplicate' is already defined."));
    }

    [Test]
    public void AddPart_String_ILogFormatPart_ShouldAddPart_WhenNameAndPartAreValid()
    {
        var builder = new LogFormatPartSetBuilder();
        var part = new MockLogFormatPart();

        var result = builder.AddPart("test", part);

        Assert.That(result, Is.SameAs(builder));
        var built = builder.Build();
        Assert.That(built.Parts.ContainsKey("test"), Is.True, "Part with name 'test' should be present.");
        Assert.That(built.Parts["test"], Is.SameAs(part));
        Assert.That(built.PartNames, Does.Contain("test"), "Part name 'test' should be in ordered list.");
        Assert.That(built.PartNames.ToList().IndexOf("test"), Is.EqualTo(0), "Part name should be first in order.");
    }

    #endregion

    #region AddPart_String_LogFormatPartType_Func_Formatter

    [Test]
    public void AddPart_String_LogFormatPartType_Func_Formatter_ShouldThrowArgumentNullException_WhenNameIsNull()
    {
        var builder = new LogFormatPartSetBuilder();
        var parser = new Func<string, object?>(_ => null);

        var ex = Assert.Throws<ArgumentNullException>(() => builder.AddPart(null!, LogFormatPartType.TEXT, parser));
        Assert.That(ex.ParamName, Is.EqualTo("name"));
    }

    [Test]
    public void AddPart_String_LogFormatPartType_Func_Formatter_ShouldThrowArgumentNullException_WhenParserIsNull()
    {
        var builder = new LogFormatPartSetBuilder();

        var ex = Assert.Throws<ArgumentNullException>(() => builder.AddPart("name", LogFormatPartType.TEXT, null!));
        Assert.That(ex.ParamName, Is.EqualTo("parser"));
    }

    [Test]
    public void AddPart_String_LogFormatPartType_Func_Formatter_ShouldThrowInvalidOperationException_WhenPartWithNameAlreadyExists()
    {
        var builder = new LogFormatPartSetBuilder();
        var parser = new Func<string, object?>(_ => null);

        builder.AddPart("existing", LogFormatPartType.TEXT, parser);

        var ex = Assert.Throws<InvalidOperationException>(() => builder.AddPart("existing", LogFormatPartType.INTEGER, parser));
        Assert.That(ex.Message, Does.Contain("A log part with name 'existing' is already defined."));
    }

    [Test]
    public void AddPart_String_LogFormatPartType_Func_Formatter_ShouldCreateAndAddDefaultPart_WhenAllParametersValid()
    {
        var builder = new LogFormatPartSetBuilder();
        var parser = new Func<string, object?>(s => s?.Length);
        var formatter = new Func<object?, string>(o => o?.ToString() ?? "null");

        var result = builder.AddPart("length", LogFormatPartType.INTEGER, parser, formatter);

        Assert.That(result, Is.SameAs(builder));
        var built = builder.Build();
        Assert.That(built.Parts.ContainsKey("length"), Is.True);
        var part = built.Parts["length"];
        Assert.That(part.Type, Is.EqualTo(LogFormatPartType.INTEGER));
        Assert.That(() => { part.Parser.TryInvoke("test", out var result); return result; }, Is.EqualTo(4));
        Assert.That(() => { part.Formatter.TryInvoke(123, out var result); return result; }, Is.EqualTo("123"));
        Assert.That(built.PartNames, Does.Contain("length"));
        Assert.That(built.PartNames.ToList().IndexOf("length"), Is.EqualTo(0));
    }

    #endregion

    #region Build

    [Test]
    public void Build_ShouldThrowInvalidOperationException_WhenNoPartsAdded()
    {
        var builder = new LogFormatPartSetBuilder();

        var ex = Assert.Throws<InvalidOperationException>(() => builder.Build());
        Assert.That(ex.Message, Does.Contain("At least one log part must be added."));
    }

    [Test]
    public void Build_ShouldReturnLogFormatPartSet_WhenOnePartAdded()
    {
        var builder = new LogFormatPartSetBuilder();
        var part = new MockLogFormatPart();

        builder.AddPart("single", part);
        var result = builder.Build();

        Assert.That(result.PartNames.Count, Is.EqualTo(1));
        Assert.That(result.PartNames[0], Is.EqualTo("single"));
        Assert.That(result.Parts.Count, Is.EqualTo(1));
        Assert.That(result.Parts["single"], Is.SameAs(part));
    }

    [Test]
    public void Build_ShouldReturnLogFormatPartSet_WhenMultiplePartsAddedInOrder()
    {
        var builder = new LogFormatPartSetBuilder();
        var part1 = new MockLogFormatPart();
        var part2 = new MockLogFormatPart();
        var part3 = new MockLogFormatPart();

        builder
            .AddPart("first", part1)
            .AddPart("second", part2)
            .AddPart("third", part3);

        var result = builder.Build();

        Assert.That(result.PartNames.Count, Is.EqualTo(3));
        Assert.That(result.PartNames, Is.EqualTo(new[] { "first", "second", "third" }));
        Assert.That(result.Parts.Count, Is.EqualTo(3));
        Assert.That(result.Parts["first"], Is.SameAs(part1));
        Assert.That(result.Parts["second"], Is.SameAs(part2));
        Assert.That(result.Parts["third"], Is.SameAs(part3));
    }

    #endregion
}
