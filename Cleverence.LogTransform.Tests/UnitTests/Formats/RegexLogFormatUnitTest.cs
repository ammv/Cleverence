using Cleverence.LogTransform.Formats;
using Cleverence.LogTransform.Tests.Helpers;
using System.Collections.ObjectModel;

namespace Cleverence.LogTransform.Tests;

public class RegexLogFormatUnitTest
{
    #region Constructor

    [Test]
    public void Constructor_ShouldThrowArgumentNullException_WhenPatternIsNull()
    {
        var partSet = LogFormatPartSetFactory.CreatePartSet(
            new[] { "ts" },
            new Dictionary<string, ILogFormatPart> { ["ts"] = new MockLogFormatPart() });

        var ex = Assert.Throws<ArgumentNullException>(() =>
            new RegexLogFormat('|', null!, partSet));
        Assert.That(ex.ParamName, Is.EqualTo("pattern"));
    }

    [Test]
    public void Constructor_ShouldThrowArgumentException_WhenGroupCountDoesNotMatchPartCount()
    {
        var partSet = LogFormatPartSetFactory.CreatePartSet(
            new[] { "a", "b" },
            new Dictionary<string, ILogFormatPart>
            {
                ["a"] = new MockLogFormatPart(),
                ["b"] = new MockLogFormatPart()
            });

        var ex = Assert.Throws<ArgumentException>(() =>
            new RegexLogFormat('|', @"(?<a>\d+)", partSet));
        Assert.That(ex.ParamName, Is.EqualTo("pattern"));
        Assert.That(ex.Message, Does.Contain("The number of regex pattern groups (1) does not match the number of parts (2)."));
    }

    [Test]
    public void Constructor_ShouldThrowArgumentException_WhenPartNameIsNotInRegexGroups()
    {
        var partSet = LogFormatPartSetFactory.CreatePartSet(
            new[] { "missing" },
            new Dictionary<string, ILogFormatPart> { ["missing"] = new MockLogFormatPart() });

        var ex = Assert.Throws<ArgumentException>(() =>
            new RegexLogFormat('|', @"(?<present>\w+)", partSet));
        Assert.That(ex.ParamName, Is.EqualTo("pattern"));
        Assert.That(ex.Message, Does.Contain("Regex pattern does not contain named group 'missing'."));
    }

    [Test]
    public void Constructor_ShouldSucceed_WhenPatternGroupsMatchPartNamesExactly()
    {
        var partSet = LogFormatPartSetFactory.CreatePartSet(
            new[] { "level", "message" },
            new Dictionary<string, ILogFormatPart>
            {
                ["level"] = new MockLogFormatPart(),
                ["message"] = new MockLogFormatPart()
            });

        var format = new RegexLogFormat(
            separator: ' ',
            pattern: @"(?<level>\w+)\s+(?<message>.+)",
            partSet: partSet);

        Assert.That(format, Is.Not.Null);
        Assert.That(format.PartNames, Has.Count.EqualTo(2));
    }

    [Test]
    public void Constructor_WithEnumerable_ShouldDelegateToPrimaryConstructor()
    {
        var parts = new ReadOnlyDictionary<string, ILogFormatPart>(
            new Dictionary<string, ILogFormatPart>
            {
                ["id"] = new MockLogFormatPart(),
                ["msg"] = new MockLogFormatPart()
            });

        var format = new RegexLogFormat(
            separator: ':',
            pattern: @"ID:(?<id>\d+);MSG:(?<msg>.+)",
            partNames: new[] { "id", "msg" },
            parts: parts);

        Assert.That(format.PartNames, Is.EqualTo(new[] { "id", "msg" }));
    }

    #endregion

    #region TryParse

    [Test]
    public void TryParse_ShouldReturnFalse_WhenLogIsNull()
    {
        var format = RegexLogFormatFactory.CreateValidRegexFormat();
        var success = format.TryParse(null, out var values);
        Assert.That(success, Is.False);
        Assert.That(values, Is.Null);
    }

    [Test]
    public void TryParse_ShouldReturnFalse_WhenLogIsEmpty()
    {
        var format = RegexLogFormatFactory.CreateValidRegexFormat();
        var success = format.TryParse("", out var values);
        Assert.That(success, Is.False);
        Assert.That(values, Is.Null);
    }

    [Test]
    public void TryParse_ShouldReturnFalse_WhenRegexDoesNotMatch()
    {
        var format = RegexLogFormatFactory.CreateValidRegexFormat(
            new MockLogFormatPart(
                parser: x =>
                {
                    return int.Parse(x);
                }
            )
        );
        var success = format.TryParse("invalid input", out var values);
        Assert.That(success, Is.False);
        Assert.That(values, Is.Null);
    }

    [Test]
    public void TryParse_ShouldReturnTrueAndParsedValues_WhenRegexMatchesAndParsersSucceed()
    {
        var intParser = new MockLogFormatPart ( parser: s => int.Parse(s!) );
        var strParser = new MockLogFormatPart (parser: s => s);

        var format = new RegexLogFormat(
            separator: ' ',
            pattern: @"User=(?<id>\d+),Action=(?<op>\w+)",
            partNames: new[] { "id", "op" },
            parts: new ReadOnlyDictionary<string, ILogFormatPart>(
                new Dictionary<string, ILogFormatPart>
                {
                    ["id"] = intParser,
                    ["op"] = strParser
                }));

        var success = format.TryParse("User=123,Action=login", out var values);

        Assert.That(success, Is.True);
        Assert.That(values, Is.Not.Null);
        Assert.That(values["id"], Is.EqualTo(123));
        Assert.That(values["op"], Is.EqualTo("login"));
    }

    [Test]
    public void TryParse_ShouldPassNullToParser_WhenGroupIsNotCaptured()
    {
        // Группа 'optional' отсутствует во входе
        var format = new RegexLogFormat(
            separator: ',',
            pattern: @"req:(?<required>\w+)(?:,opt:(?<optional>\w+))?",
            partNames: new[] { "required", "optional" },
            parts: new ReadOnlyDictionary<string, ILogFormatPart>(
                new Dictionary<string, ILogFormatPart>
                {
                    ["required"] = new MockLogFormatPart (parser: s => s),
                    ["optional"] = new MockLogFormatPart (parser: s => s ?? "(none)")
                }));

        var success = format.TryParse("req:GET", out var values);

        Assert.That(success, Is.True);
        Assert.That(values?["required"], Is.EqualTo("GET"));
        Assert.That(values?["optional"], Is.EqualTo("(none)"));
    }

    [Test]
    public void TryParse_ShouldReturnFalse_WhenParserFailsOnCapturedGroup()
    {
        var failingParser = new MockLogFormatPart ( parser: _ => throw new FormatException() );
        var okParser = new MockLogFormatPart (parser: s => s);

        var format = new RegexLogFormat(
            separator: '=',
            pattern: @"(?<key>\w+)=(?<value>\w+)",
            partNames: new[] { "key", "value" },
            parts: new ReadOnlyDictionary<string, ILogFormatPart>(
                new Dictionary<string, ILogFormatPart>
                {
                    ["key"] = okParser,
                    ["value"] = failingParser
                }));

        var success = format.TryParse("name=test", out var values);

        Assert.That(success, Is.False);
        Assert.That(values, Is.Null);
    }

    [Test]
    public void TryParse_ShouldReturnFalse_WhenParserFailsOnNullForMissingGroup()
    {
        var failingOnNullParser = new MockLogFormatPart
        (
            parser: s =>
            {
                if (s == null) throw new InvalidOperationException("Null not allowed");
                return s;
            }
        );

        var format = new RegexLogFormat(
            separator: ' ',
            pattern: @"(?<main>\w+)(?:\s+(?<extra>\w+))?",
            partNames: new[] { "main", "extra" },
            parts: new ReadOnlyDictionary<string, ILogFormatPart>(
                new Dictionary<string, ILogFormatPart>
                {
                    ["main"] = new MockLogFormatPart (parser: s => s),
                    ["extra"] = failingOnNullParser
                }));

        var success = format.TryParse("only", out var values);

        Assert.That(success, Is.False);
        Assert.That(values, Is.Null);
    }

    [Test]
    public void TryParse_ShouldPreservePartOrderIndependentlyOfRegexGroupOrder()
    {
        var format = new RegexLogFormat(
            separator: ';',
            pattern: @"A:(?<a>\d+);B:(?<b>\w+)",
            partNames: new[] { "b", "a" },
            parts: new ReadOnlyDictionary<string, ILogFormatPart>(
                new Dictionary<string, ILogFormatPart>
                {
                    ["a"] = new MockLogFormatPart (parser: s => s),
                    ["b"] = new MockLogFormatPart (parser: s => s)
                }));

        var success = format.TryParse("A:42;B:hello", out var values);

        Assert.That(success, Is.True);
        Assert.That(values?["b"], Is.EqualTo("hello"));
        Assert.That(values?["a"], Is.EqualTo("42"));
    }

    #endregion

    #region Equality

    [Test]
    public void Equals_ShouldReturnFalse_WhenRegexPatternsDiffer()
    {
        var parts = new ReadOnlyDictionary<string, ILogFormatPart>(
            new Dictionary<string, ILogFormatPart> { ["f"] = new MockLogFormatPart() });

        var format1 = new RegexLogFormat('|', @"(?<f>\d+)", new[] { "f" }, parts);
        var format2 = new RegexLogFormat('|', @"(?<f>\w+)", new[] { "f" }, parts);

        Assert.That(format1.Equals(format2), Is.False);
        Assert.That(format1.GetHashCode(), Is.Not.EqualTo(format2.GetHashCode()));
    }

    [Test]
    public void Equals_ShouldReturnTrue_WhenRegexAndAllBaseComponentsAreEqual()
    {
        var parts = new ReadOnlyDictionary<string, ILogFormatPart>(
            new Dictionary<string, ILogFormatPart> { ["x"] = new MockLogFormatPart() });

        var format1 = new RegexLogFormat(',', @"(?<x>.+)", new[] { "x" }, parts);
        var format2 = new RegexLogFormat(',', @"(?<x>.+)", new[] { "x" }, parts);

        Assert.That(format1, Is.EqualTo(format2));
        Assert.That(format1.GetHashCode(), Is.EqualTo(format2.GetHashCode()));
    }

    #endregion
}
