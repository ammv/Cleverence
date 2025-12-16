using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cleverence.LogTransform.ConsoleApp.Tests.IntegrationTests
{
    [TestFixture, Category("Integration")]
    internal class ErrorHandlingIntegrationTest
    {
        private string _tempDir = "";
        private StringWriter _errorWriter;

        [SetUp]
        public void Setup()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDir);

            // Перенаправляем Console.Error в StringWriter
            _errorWriter = new StringWriter();
            Console.SetError(_errorWriter);
        }

        [TearDown]
        public void Teardown()
        {
            // Восстанавливаем оригинальный Console.Error
            var standardError = new StreamWriter(Console.OpenStandardError()) { AutoFlush = true };
            Console.SetError(standardError);

            _errorWriter?.Dispose();

            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, true);
        }

        [Test]
        public void PerformLogTransformation_InputFileDoesNotExist_ShouldWriteErrorToConsoleError()
        {
            // Arrange
            var inputPath = Path.Combine(_tempDir, "nonexistent.log");
            var outputPath = Path.Combine(_tempDir, "output.log");
            var problemPath = Path.Combine(_tempDir, "problems.log");

            // Act
            Program.PerformLogTransformation(inputPath, outputPath, problemPath);

            // Assert
            var errorOutput = _errorWriter.ToString();
            Assert.That(errorOutput, Does.Contain("Input file not found"));
            Assert.That(errorOutput, Does.Contain("nonexistent.log"));
        }

        [Test]
        public void PerformLogTransformation_OutputFileInUse_ShouldWriteErrorToConsoleError()
        {
            // Arrange
            var inputPath = Path.Combine(_tempDir, "input.log");
            var outputPath = Path.Combine(_tempDir, "output.log");
            var problemPath = Path.Combine(_tempDir, "problems.log");

            File.WriteAllText(inputPath, "10.03.2025 15:14:49.523 INFORMATION Valid log");

            // Блокируем выходной файл в этом же процессе (реальная блокировка, но БЕЗ зависания!)
            using var lockedStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);

            // Act
            Program.PerformLogTransformation(inputPath, outputPath, problemPath);

            // Assert
            var errorOutput = _errorWriter.ToString();
            Assert.That(errorOutput, Does.Contain("Failed to process logs: The process cannot access the file"));
            Assert.That(errorOutput, Does.Contain("output.log"));
        }

        [Test]
        public void PerformLogTransformation_ProblemFileInUse_ShouldWriteErrorToConsoleError()
        {
            // Arrange
            var inputPath = Path.Combine(_tempDir, "input.log");
            var outputPath = Path.Combine(_tempDir, "output.log");
            var problemPath = Path.Combine(_tempDir, "problems.log");

            File.WriteAllText(inputPath, "invalid log line");

            // Блокируем проблемный файл
            using var lockedStream = new FileStream(problemPath, FileMode.Create, FileAccess.Write, FileShare.None);

            // Act
            Program.PerformLogTransformation(inputPath, outputPath, problemPath);

            // Assert
            var errorOutput = _errorWriter.ToString();
            Assert.That(errorOutput, Does.Contain("Failed to process logs: The process cannot access the file"));
            Assert.That(errorOutput, Does.Contain("problems.log"));
        }
    }
}
