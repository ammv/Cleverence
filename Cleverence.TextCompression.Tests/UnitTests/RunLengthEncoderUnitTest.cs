namespace Cleverence.TextCompression.Tests;

public class RunLengthEncoderUnitTest
{
    private RunLengthEncoder _encoder;
    private Dictionary<string, string> _correctInputCompressMap;

    [SetUp]
    public void Setup()
    {
        _encoder = new RunLengthEncoder();
        _correctInputCompressMap = new()
        {
            {"aaa", "a3"},
            {"aaabbc", "a3b2c"},
            {"abcabc", "abcabc"},
            {"abcdefghijklmnopqrstuvwxyz", "abcdefghijklmnopqrstuvwxyz"}
        };
    }

    #region Compress tests

    [Test]
    public void Compress_ReturnsEmptyString_WhenInputIsNull()
    {
        // Arrange
        string input = null;
        string expected = String.Empty;

        // Act
        string result = _encoder.Compress(input);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void Compress_ReturnsEmptyString_WhenInputIsEmpty()
    {
        // Arrange
        string input = "";
        string expected = String.Empty;

        // Act
        string result = _encoder.Compress(input);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void Compress_ThrowsArgumentException_WhenInputHasNotAllowedChars()
    {
        // Arrange
        Dictionary<string, char> incorrectInputMap = new Dictionary<string, char>()
        {
            {"A",'A'},
            {"bbbB", 'B'},
            {"aaa3", '3'},
            {"abcdefghijklm nopqrstuvwxyz", ' '}
        };

        string expectedExceptionParamName = "input";
        string expectedExceptionMessageTemplate = $"Uncompressed input contains an invalid character: '#'. " +
                            $"Only lowercase letters (a-z) are allowed.";

        Func<char, string> getExpectedExceptionMessage = (c) => expectedExceptionMessageTemplate.Replace('#', c);

        // Act && Assert
        foreach (var pair in incorrectInputMap)
        {
            ArgumentException ex = Assert.Throws<ArgumentException>(() => _encoder.Compress(pair.Key));

            Assert.That(ex.ParamName, Is.EqualTo(expectedExceptionParamName));

            Assert.That(ex.Message, Does.StartWith(getExpectedExceptionMessage(pair.Value)));
        }
    }

    [Test]
    public void Compress_ReturnsCompressedString_WhenInputIsCorrect()
    {
        // Act && Assert
        foreach (var pair in _correctInputCompressMap)
        {
            Assert.That(pair.Value, Is.EqualTo(_encoder.Compress(pair.Key)));
        }
    }

    [Test]
    public void Compress_ReturnsCompressedString_WhenInputIsMaxLengthString()
    {
        // Arrange
        string input = new string('a', RunLengthEncoder.MaxStringLength);
        string expected = $"a{RunLengthEncoder.MaxStringLength}";

        // Act
        string result = _encoder.Compress(input);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    #endregion

    #region Decompress tests

    [Test]
    public void Decompress_ReturnsEmptyString_WhenInputIsNull()
    {
        // Arrange
        string input = null;
        string expected = String.Empty;

        // Act
        string result = _encoder.Decompress(input);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void Decompress_ReturnsEmptyString_WhenInputIsEmpty()
    {
        // Arrange
        string input = "";
        string expected = String.Empty;

        // Act
        string result = _encoder.Decompress(input);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void Decompress_ReturnsDecompressedString_WhenInputIsCorrect()
    {
        // Act && Assert
        foreach (var pair in _correctInputCompressMap)
        {
            Assert.That(pair.Key, Is.EqualTo(_encoder.Decompress(pair.Value)));
        }
    }

    [Test]
    public void Decompress_ThrowsArgumentException_WhenInputHasNotAllowedChars()
    {
        // Arrange
        Dictionary<string, char> incorrectInputMap = new Dictionary<string, char>()
        {
            {"A",'A'},
            {"bbbB", 'B'},
            {"aaa3C", 'C'},
            {"abcdefghijklmnopqrstuvwxyzZ", 'Z'}
        };

        string expectedExceptionParamName = "input";
        string expectedExceptionMessageTemplate = $"Compressed input contains an invalid character: '#'. " +
                            $"Only lowercase letters (a-z) and digits are allowed.";

        Func<char, string> getExpectedExceptionMessage = (c) => expectedExceptionMessageTemplate.Replace('#', c);

        // Act && Assert
        foreach (var pair in incorrectInputMap)
        {
            ArgumentException ex = Assert.Throws<ArgumentException>(() => _encoder.Decompress(pair.Key));

            Assert.That(ex.ParamName, Is.EqualTo(expectedExceptionParamName));

            Assert.That(ex.Message, Does.StartWith(getExpectedExceptionMessage(pair.Value)));
        }
    }

    [Test]
    public void Decompress_ThrowsArgumentException_WhenCompressedInputHasZeroCount()
    {
        // Arrange
        string input = "a3b2c00000";

        // Act && Assert
        ArgumentException ex = Assert.Throws<ArgumentException>(() => _encoder.Decompress(input));
        Assert.That(ex.Message, Does.StartWith($"Count can`t be zero in compressed data: 0"));
    }

    [Test]
    public void Decompress_ThrowsOverflowException_WhenNumbersInCompressedStringGreaterThanMaxStringSize()
    {
        // Arrange
        string[] inputs = {
            $"a{RunLengthEncoder.MaxStringLength+1}",
            $"a{RunLengthEncoder.MaxStringLength}a",
            $"a{RunLengthEncoder.MaxStringLength/2}b{RunLengthEncoder.MaxStringLength}",
        };

        // Act && Assert
        foreach (var input in inputs)
        {
            Assert.Throws<OverflowException>(() => _encoder.Decompress(input));
        }
    }

    [Test]
    public void Decompress_ReturnsString_WhenInputIsMaxLengthCompressedString()
    {
        // Arrange
        string input = $"a{RunLengthEncoder.MaxStringLength}";
        int expectedLength = RunLengthEncoder.MaxStringLength;

        // Act
        string result = _encoder.Decompress(input);

        // Assert
        Assert.That(result[0], Is.EqualTo('a'));
        Assert.That(result.Length, Is.EqualTo(expectedLength));
    }

    #endregion
}
