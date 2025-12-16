using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cleverence.LogTransform.ConsoleApp.Tests.IntegrationTests
{
    [TestFixture, Category("Integration")]
    internal class LogTransformationIntegrationTest
    {
        private string _tempDir = "";

        [SetUp]
        public void Setup()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDir);
        }

        [TearDown]
        public void Teardown()
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, true);
        }

        [Test]
        public void EndToEnd_ValidAndInvalidLogs_ShouldProduceCorrectOutput()
        {
            var input = Path.Combine(_tempDir, "input.log");
            var output = Path.Combine(_tempDir, "output.log");
            var problems = Path.Combine(_tempDir, "problems.log");

            File.WriteAllLines(input, new[]
            {
                "10.03.2025 15:14:49.523 INFORMATION Valid",
                "invalid line",
                "2025-03-10 15:14:51.5882| INFO|11|MobileComputer.GetDeviceId| Valid2"
            });

            Program.PerformLogTransformation(input, output, problems);

            var outputLines = File.ReadAllLines(output);
            var problemLines = File.ReadAllLines(problems);

            Assert.That(outputLines.Length, Is.EqualTo(2));
            Assert.That(problemLines.Length, Is.EqualTo(1));
            Assert.That(problemLines[0], Is.EqualTo("invalid line"));
        }
    }
}
