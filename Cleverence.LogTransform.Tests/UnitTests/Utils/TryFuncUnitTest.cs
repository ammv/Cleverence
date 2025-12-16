using Cleverence.LogTransform.Utils;

namespace Cleverence.LogTransform.Tests.UnitTests.Utils
{
    [TestFixture]
    public class TryFuncUnitTest
    {
        #region Constructor

        [Test]
        public void Constructor_ShouldThrowArgumentNullException_WhenFuncIsNull()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new TryFunc<int, string>(null!));
            Assert.That(ex.ParamName, Is.EqualTo("func"));
        }

        [Test]
        public void Constructor_ShouldStoreFunc_WhenFuncIsProvided()
        {
            var func = new Func<int, string>(i => i.ToString());
            var tryFunc = new TryFunc<int, string>(func);

            Assert.That(tryFunc, Is.Not.Null);
        }

        #endregion

        #region TryInvoke

        [Test]
        public void TryInvoke_ShouldReturnTrueAndCorrectResult_WhenFuncExecutesSuccessfully()
        {
            var func = new Func<int, string>(i => $"value:{i}");
            var tryFunc = new TryFunc<int, string>(func);

            var success = tryFunc.TryInvoke(42, out var result);

            Assert.That(success, Is.True, "TryInvoke should return true on successful execution.");
            Assert.That(result, Is.EqualTo("value:42"));
        }

        [Test]
        public void TryInvoke_ShouldReturnFalseAndDefaultResult_WhenFuncThrowsException()
        {
            var func = new Func<int, string>(_ => throw new InvalidOperationException("Boom!"));
            var tryFunc = new TryFunc<int, string>(func);

            var success = tryFunc.TryInvoke(0, out var result);

            Assert.That(success, Is.False, "TryInvoke should return false when function throws.");
            Assert.That(result, Is.EqualTo(default(string)));
        }

        [Test]
        public void TryInvoke_ShouldReturnDefaultOfTResult_WhenFuncThrowsAndTResultIsValueType()
        {
            var func = new Func<string, int>(_ => throw new ArgumentException());
            var tryFunc = new TryFunc<string, int>(func);

            var success = tryFunc.TryInvoke("invalid", out var result);

            Assert.That(success, Is.False);
            Assert.That(result, Is.EqualTo(default(int)));
        }

        [Test]
        public void TryInvoke_ShouldNotRethrowException_WhenFuncThrows()
        {
            var func = new Func<object, object>(_ => throw new NullReferenceException());
            var tryFunc = new TryFunc<object, object>(func);

            Assert.DoesNotThrow(() => tryFunc.TryInvoke(null, out _));
        }

        [Test]
        public void TryInvoke_ShouldInvokeFuncExactlyOnce_WhenCalled()
        {
            var callCount = 0;
            var func = new Func<int, int>(i =>
            {
                callCount++;
                return i * 2;
            });
            var tryFunc = new TryFunc<int, int>(func);

            tryFunc.TryInvoke(5, out _);

            Assert.That(callCount, Is.EqualTo(1), "The wrapped function should be invoked exactly once.");
        }

        #endregion
    }
}
