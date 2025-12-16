using Cleverence.LogTransform.EqualityComparers;
using Cleverence.LogTransform.Formats;
using Moq;

namespace Cleverence.LogTransform.Tests;

[TestFixture]
public class LogFormatEqualityComparerUnitTest
{
    private LogFormatEqualityComparer _comparer;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _comparer = LogFormatEqualityComparer.Instance;
    }

    #region Equals

    [Test]
    public void Equals_BothNull_ReturnsTrue()
    {
        var result = _comparer.Equals(null, null);
        Assert.That(result, Is.True, "Expected Equals(null, null) to return true.");
    }

    [Test]
    public void Equals_FirstNull_SecondNotNull_ReturnsFalse()
    {
        var mockFormat = new Mock<ILogFormat>();
        var result = _comparer.Equals(null, mockFormat.Object);
        Assert.That(result, Is.False, "Expected Equals(null, valid) to return false.");
    }

    [Test]
    public void Equals_FirstNotNull_SecondNull_ReturnsFalse()
    {
        var mockFormat = new Mock<ILogFormat>();
        var result = _comparer.Equals(mockFormat.Object, null);
        Assert.IsFalse(result, "Expected Equals(valid, null) to return false.");
    }

    [Test]
    public void Equals_SameReference_ReturnsTrue()
    {
        var mockFormat = new Mock<ILogFormat>().Object;
        var result = _comparer.Equals(mockFormat, mockFormat);
        Assert.That(result, Is.True, "Expected Equals(x, x) to return true.");
    }

    [Test]
    public void Equals_SameTypesAndDiffrentSeparator_ReturnsFalse()
    {
        var mockFormat1 = new Mock<ILogFormat>();
        var mockFormat2 = new Mock<ILogFormat>();

        mockFormat1.Setup(f => f.Separator).Returns('\t');
        mockFormat2.Setup(f => f.Separator).Returns('\n');

        var result = _comparer.Equals(mockFormat1.Object, mockFormat2.Object);
        Assert.That(result, Is.False, "Expected Equals(x, y) to return false when Separator diff even if types matches.");
    }

    [Test]
    public void Equals_SameTypesAndDiffrentPartNamesCount_ReturnsFalse()
    {
        var mockFormat1 = new Mock<ILogFormat>();
        var mockFormat2 = new Mock<ILogFormat>();

        mockFormat1.Setup(f => f.PartNames).Returns(["test"]);
        mockFormat2.Setup(f => f.PartNames).Returns(["test", "test2"]);

        var result = _comparer.Equals(mockFormat1.Object, mockFormat2.Object);
        Assert.That(result, Is.False, "Expected Equals(x, y) to return false when PartNames count diff even if types matches.");
    }

    [Test]
    public void Equals_SameTypesAndDiffrentPartName_ReturnsFalse()
    {
        var mockFormat1 = new Mock<ILogFormat>();
        var mockFormat2 = new Mock<ILogFormat>();

        mockFormat1.Setup(f => f.PartNames).Returns(["test", "test1"]);
        mockFormat2.Setup(f => f.PartNames).Returns(["test", "test2"]);

        var result = _comparer.Equals(mockFormat1.Object, mockFormat2.Object);
        Assert.That(result, Is.False, "Expected Equals(x, y) to return false when second part name count diff even if types matches.");
    }

    [Test]
    public void Equals_SameTypesAndDiffrentPartType_ReturnsFalse()
    {
        var mockFormat1 = new Mock<ILogFormat>();
        var mockFormat2 = new Mock<ILogFormat>();

        var mockFormatLogPart1 = new Mock<ILogFormatPart>();
        var mockFormatLogPart2 = new Mock<ILogFormatPart>();

        mockFormat1.Setup(f => f.PartNames).Returns(["test"]);
        mockFormat2.Setup(f => f.PartNames).Returns(["test"]);

        mockFormatLogPart1.Setup(f => f.Type).Returns(Models.LogFormatPartType.TEXT);
        mockFormatLogPart2.Setup(f => f.Type).Returns(Models.LogFormatPartType.FLOAT);

        mockFormat1.Setup(f => f["test"]).Returns(mockFormatLogPart1.Object);
        mockFormat2.Setup(f => f["test"]).Returns(mockFormatLogPart2.Object);

        var result = _comparer.Equals(mockFormat1.Object, mockFormat2.Object);
        Assert.That(result, Is.False, "Expected Equals(x, y) to return false when part type count diff even if types matches.");
    }

    [Test]
    public void Equals_SameTypes_ReturnsTrue()
    {
        var mockFormat1 = new Mock<ILogFormat>();
        var mockFormat2 = new Mock<ILogFormat>();

        var mockFormatLogPart1 = new Mock<ILogFormatPart>();
        var mockFormatLogPart2 = new Mock<ILogFormatPart>();

        mockFormat1.Setup(f => f.PartNames).Returns(["test"]);
        mockFormat2.Setup(f => f.PartNames).Returns(["test"]);

        mockFormatLogPart1.Setup(f => f.Type).Returns(Models.LogFormatPartType.TEXT);
        mockFormatLogPart2.Setup(f => f.Type).Returns(Models.LogFormatPartType.TEXT);

        mockFormat1.Setup(f => f["test"]).Returns(mockFormatLogPart1.Object);
        mockFormat2.Setup(f => f["test"]).Returns(mockFormatLogPart2.Object);

        var result = _comparer.Equals(mockFormat1.Object, mockFormat2.Object);
        Assert.That(result, Is.True, "Expected Equals(x, y) to return true when types matches.");
    }

    #endregion

    #region GetHashCode

    [Test]
    public void GetHashCode_Null_ReturnsZero()
    {
        var result = _comparer.GetHashCode(null);

        Assert.That(result, Is.EqualTo(0), "Expected GetHashCode(null) to return 0.");
    }

    [Test]
    public void GetHashCode_DiffSeparator_SameType_ReturnsDiffHashCode()
    {
        var mockFormat1 = new Mock<ILogFormat>();
        var mockFormat2 = new Mock<ILogFormat>();

        var mockFormatLogPart1 = new Mock<ILogFormatPart>();
        var mockFormatLogPart2 = new Mock<ILogFormatPart>();

        mockFormat1.Setup(f => f.Separator).Returns('\t');
        mockFormat2.Setup(f => f.Separator).Returns('\n');

        mockFormat1.Setup(f => f.PartNames).Returns(["test"]);
        mockFormat2.Setup(f => f.PartNames).Returns(["test"]);

        mockFormatLogPart1.Setup(f => f.Type).Returns(Models.LogFormatPartType.TEXT);
        mockFormatLogPart2.Setup(f => f.Type).Returns(Models.LogFormatPartType.TEXT);

        mockFormat1.Setup(f => f["test"]).Returns(mockFormatLogPart1.Object);
        mockFormat2.Setup(f => f["test"]).Returns(mockFormatLogPart2.Object);

        var hash1 = _comparer.GetHashCode(mockFormat1.Object);
        var hash2 = _comparer.GetHashCode(mockFormat2.Object);

        Assert.That(hash1, Is.Not.EqualTo(hash2), "Expected GetHashCode to return diffrent value for equal instances with diffrent separators.");
    }

    [Test]
    public void GetHashCode_DiffPartName_SameType_ReturnsDiffHashCode()
    {
        var mockFormat1 = new Mock<ILogFormat>();
        var mockFormat2 = new Mock<ILogFormat>();

        var mockFormatLogPart1 = new Mock<ILogFormatPart>();
        var mockFormatLogPart2 = new Mock<ILogFormatPart>();

        mockFormat1.Setup(f => f.Separator).Returns('\t');
        mockFormat2.Setup(f => f.Separator).Returns('\t');

        mockFormat1.Setup(f => f.PartNames).Returns(["test"]);
        mockFormat2.Setup(f => f.PartNames).Returns(["test2"]);

        mockFormatLogPart1.Setup(f => f.Type).Returns(Models.LogFormatPartType.TEXT);
        mockFormatLogPart2.Setup(f => f.Type).Returns(Models.LogFormatPartType.TEXT);

        mockFormat1.Setup(f => f["test"]).Returns(mockFormatLogPart1.Object);
        mockFormat2.Setup(f => f["test2"]).Returns(mockFormatLogPart2.Object);

        var hash1 = _comparer.GetHashCode(mockFormat1.Object);
        var hash2 = _comparer.GetHashCode(mockFormat2.Object);

        Assert.That(hash1, Is.Not.EqualTo(hash2), "Expected GetHashCode to return diffrent value for equal instances with diffrent part name.");
    }

    [Test]
    public void GetHashCode_DiffPartType_SameType_ReturnsDiffHashCode()
    {
        var mockFormat1 = new Mock<ILogFormat>();
        var mockFormat2 = new Mock<ILogFormat>();

        var mockFormatLogPart1 = new Mock<ILogFormatPart>();
        var mockFormatLogPart2 = new Mock<ILogFormatPart>();

        mockFormat1.Setup(f => f.Separator).Returns('\t');
        mockFormat2.Setup(f => f.Separator).Returns('\t');

        mockFormat1.Setup(f => f.PartNames).Returns(["test"]);
        mockFormat2.Setup(f => f.PartNames).Returns(["test"]);

        mockFormatLogPart1.Setup(f => f.Type).Returns(Models.LogFormatPartType.TEXT);
        mockFormatLogPart2.Setup(f => f.Type).Returns(Models.LogFormatPartType.FLOAT);

        mockFormat1.Setup(f => f["test"]).Returns(mockFormatLogPart1.Object);
        mockFormat2.Setup(f => f["test"]).Returns(mockFormatLogPart2.Object);

        var hash1 = _comparer.GetHashCode(mockFormat1.Object);
        var hash2 = _comparer.GetHashCode(mockFormat2.Object);

        Assert.That(hash1, Is.Not.EqualTo(hash2), "Expected GetHashCode to return diffrent value for equal instances with diffrent part type.");
    }

    [Test]
    public void GetHashCode_SameType_ReturnsSameHashCode()
    {
        var mockFormat1 = new Mock<ILogFormat>();
        var mockFormat2 = new Mock<ILogFormat>();

        var mockFormatLogPart1 = new Mock<ILogFormatPart>();
        var mockFormatLogPart2 = new Mock<ILogFormatPart>();

        mockFormat1.Setup(f => f.Separator).Returns('\t');
        mockFormat2.Setup(f => f.Separator).Returns('\t');

        mockFormat1.Setup(f => f.PartNames).Returns(["test"]);
        mockFormat2.Setup(f => f.PartNames).Returns(["test"]);

        mockFormatLogPart1.Setup(f => f.Type).Returns(Models.LogFormatPartType.TEXT);
        mockFormatLogPart2.Setup(f => f.Type).Returns(Models.LogFormatPartType.TEXT);

        mockFormat1.Setup(f => f["test"]).Returns(mockFormatLogPart1.Object);
        mockFormat2.Setup(f => f["test"]).Returns(mockFormatLogPart2.Object);

        var hash1 = _comparer.GetHashCode(mockFormat1.Object);
        var hash2 = _comparer.GetHashCode(mockFormat2.Object);

        Assert.That(hash1, Is.EqualTo(hash2), "Expected GetHashCode to return same value for equal instances.");
    }

    #endregion
}
