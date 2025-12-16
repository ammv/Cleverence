using Cleverence.LogTransform.Formats;
using Cleverence.LogTransform.Tests.Helpers;
using System.Collections.ObjectModel;

namespace Cleverence.LogTransform.Tests.UnitTests.Formats
{
    [TestFixture]
    public class LogFormatPartSetUnitTest
    {
        #region Constructor

        [Test]
        public void Constructor_ShouldThrowArgumentNullException_WhenPartNamesIsNull()
        {
            var parts = new ReadOnlyDictionary<string, ILogFormatPart>(
                new Dictionary<string, ILogFormatPart>());

            var ex = Assert.Throws<ArgumentNullException>(() =>
                new LogFormatPartSet(null!, parts));
            Assert.That(ex.ParamName, Is.EqualTo("partNames"));
        }

        [Test]
        public void Constructor_ShouldThrowArgumentNullException_WhenPartsIsNull()
        {
            var partNames = new List<string>();

            var ex = Assert.Throws<ArgumentNullException>(() =>
                new LogFormatPartSet(partNames, null!));
            Assert.That(ex.ParamName, Is.EqualTo("parts"));
        }

        [Test]
        public void Constructor_ShouldThrowArgumentException_WhenPartNamesAndPartsHaveDifferentCounts()
        {
            var partNames = new List<string> { "a", "b" };
            var parts = new ReadOnlyDictionary<string, ILogFormatPart>(
                new Dictionary<string, ILogFormatPart> { ["a"] = new MockLogFormatPart() });

            var ex = Assert.Throws<ArgumentException>(() =>
                new LogFormatPartSet(partNames, parts));
            Assert.That(ex.Message, Does.Contain("PartNames and Parts must have the same number of elements."));
        }

        [Test]
        public void Constructor_ShouldThrowArgumentException_WhenPartNameInPartNamesIsNotInParts()
        {
            var partNames = new List<string> { "missing" };
            var parts = new ReadOnlyDictionary<string, ILogFormatPart>(
                new Dictionary<string, ILogFormatPart> { ["present"] = new MockLogFormatPart() });

            var ex = Assert.Throws<ArgumentException>(() =>
                new LogFormatPartSet(partNames, parts));
            Assert.That(ex.Message, Does.Contain("Part name 'missing' is listed in PartNames but not in Parts."));
        }

        [Test]
        public void Constructor_ShouldInitializeProperties_WhenValidInputsProvided()
        {
            var part = new MockLogFormatPart();
            var partNames = new List<string> { "timestamp", "level" };
            var parts = new ReadOnlyDictionary<string, ILogFormatPart>(
                new Dictionary<string, ILogFormatPart>
                {
                    ["timestamp"] = part,
                    ["level"] = part
                });

            var partSet = new LogFormatPartSet(partNames, parts);

            Assert.That(partSet.PartNames, Is.SameAs(partNames));
            Assert.That(partSet.Parts, Is.SameAs(parts));
        }

        [Test]
        public void Constructor_ShouldAcceptEmptyInputs_WhenBothAreEmpty()
        {
            var partNames = new List<string>();
            var parts = new ReadOnlyDictionary<string, ILogFormatPart>(
                new Dictionary<string, ILogFormatPart>());

            var partSet = new LogFormatPartSet(partNames, parts);

            Assert.That(partSet.PartNames.Count, Is.EqualTo(0));
            Assert.That(partSet.Parts.Count, Is.EqualTo(0));
        }

        #endregion
    }
}
