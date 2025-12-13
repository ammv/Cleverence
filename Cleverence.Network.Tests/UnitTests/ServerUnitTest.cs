using NUnit.Framework;
using System.Reflection;

namespace Cleverence.Network.Tests;

[TestFixture]
public class ServerUnitTest
{
    private Action<int> SetCountFieldValue;

    [SetUp]
    public void Setup()
    {
        var countFieldAccessor = typeof(Server).GetField("_count", 
            System.Reflection.BindingFlags.Static | 
            System.Reflection.BindingFlags.NonPublic);

        ArgumentNullException.ThrowIfNull(countFieldAccessor);

        SetCountFieldValue = (x) => countFieldAccessor.SetValue(typeof(Server), x);
    }

    [TearDown]
    public void TearDown()
    {
        SetCountFieldValue(0);
    }

    #region GetCount
    [Test]
    public void GetCount_ReturnZeroCount_WhenNoChanges()
    {
        // Arrange
        int expected = 0;

        // Act
        int count = Server.GetCount();

        // Assert
        Assert.That(count, Is.EqualTo(expected));
    }
    #endregion

    #region AddToCount
    [Test]
    public void AddToCount_ChangeCount_WhenCalled()
    {
        // Arrange
        int value = 15;

        // Act
        Server.AddToCount(value);
        int result = Server.GetCount();

        // Assert
        Assert.That(result, Is.EqualTo(value));
    }
    #endregion
}
