using Cleverence.LogTransform.ConsoleApp;
using System.Reflection;

/// <summary>
/// Entry point for the LogTransform console application.
/// Provides a command-line interface to convert structured log files from supported input formats
/// to a unified tab-separated output format, while capturing unparsable lines in a separate file.
/// </summary>
/// <remarks>
/// <para>
/// This application supports two input log formats and produces a standardized output.
/// It is designed for batch processing of log files in automated environments or manual use.
/// </para>
/// <para>
/// The application follows conventional CLI patterns:
/// <list type="bullet">
///   <item><description>Uses <c>stderr</c> for error messages and diagnostics.</description></item>
///   <item><description>Overwrites output files if they exist (no append mode).</description></item>
///   <item><description>Exits with success even if some lines fail to parse (fail-soft behavior).</description></item>
/// </list>
/// </para>
/// </remarks>
public class Program
{
    /// <summary>
    /// The main entry point of the application.
    /// </summary>
    /// <param name="args">
    /// Command-line arguments:
    /// <list type="table">
    ///   <listheader>
    ///     <term>Argument</term>
    ///     <description>Description</description>
    ///   </listheader>
    ///   <item>
    ///     <term><c>help</c></term>
    ///     <description>Displays usage information and supported formats.</description>
    ///   </item>
    ///   <item>
    ///     <term><c>&lt;InputLogFilePath&gt; &lt;OutputLogFilePath&gt; &lt;ProblemLogFilePath&gt;</c></term>
    ///     <description>Processes logs from input file, writes valid entries to output, and failed entries to problems.</description>
    ///   </item>
    /// </list>
    /// </param>
    /// <remarks>
    /// <para>
    /// Expected invocation:
    /// <code>
    /// LogTransform input.log output.log problems.log
    /// </code>
    /// </para>
    /// <para>
    /// If called with no arguments or <c>"help"</c>, displays help and exits successfully.
    /// If called with an invalid number of arguments, writes an error to <c>stdout</c> and exits.
    /// </para>
    /// </remarks>
    public static void Main(string[] args)
    {
        if (args.Length == 0 || args[0].Equals("help"))
            ShowHelp();
        else if (args.Length == 3)
            PerformLogTransformation(args[0], args[1], args[2]);
        else
            Console.WriteLine("Invalid command or arguments.");
    }

    /// <summary>
    /// Displays comprehensive help information including usage, arguments, supported formats, and examples.
    /// </summary>
    /// <remarks>
    /// This method writes to <see cref="Console.Out"/> and is intended for user guidance.
    /// It includes concrete examples of valid log lines for both input formats and the corresponding output.
    /// </remarks>
    private static void ShowHelp()
    {
        Console.WriteLine("LogTransform - Converts structured log files between formats");
        Console.WriteLine();
        Console.WriteLine("USAGE:");
        Console.WriteLine("    LogTransform [OPTIONS] <INPUT> <OUTPUT> <PROBLEMS>");
        Console.WriteLine();
        Console.WriteLine("ARGUMENTS:");
        Console.WriteLine("    <INPUT>          Path to the input log file to process");
        Console.WriteLine("    <OUTPUT>         Path to the output file for successfully transformed logs");
        Console.WriteLine("    <PROBLEMS>       Path to the file for logs that failed to parse");
        Console.WriteLine();
        Console.WriteLine("OPTIONS:");
        Console.WriteLine("    help             Show this help message and exit");
        Console.WriteLine();
        Console.WriteLine("SUPPORTED INPUT FORMATS:");
        Console.WriteLine($"    Format 1:        {LogFormatExamples.InputLogFormat1Example}");
        Console.WriteLine($"    Format 2:        {LogFormatExamples.InputLogFormat2Example}");
        Console.WriteLine();
        Console.WriteLine("OUTPUT FORMAT:");
        Console.WriteLine("    All successfully parsed logs are converted to tab-separated format:");
        Console.WriteLine($"    For format 1:    {LogFormatExamples.OutputLogFormatForInputLogFormat1Example}");
        Console.WriteLine($"    For format 2:    {LogFormatExamples.OutputLogFormatForInputLogFormat2Example}");
        Console.WriteLine();
        Console.WriteLine("NOTES:");
        Console.WriteLine("    • Output and problem files will be overwritten if they exist");
        Console.WriteLine("    • Invalid log lines are written to the problem file unchanged");
        Console.WriteLine("    • Processing stops on fatal errors (e.g., missing input file)");
        Console.WriteLine();
        Console.WriteLine("EXAMPLE:");
        Console.WriteLine("    LogTransform app.log transformed.log errors.log");
    }

    /// <summary>
    /// Processes a log file by parsing, transforming, and writing results to output files.
    /// </summary>
    /// <param name="inputLogPath">The path to the input log file. Must exist.</param>
    /// <param name="outputLogPath">The path where successfully transformed logs will be written. Existing content will be overwritten.</param>
    /// <param name="problemLogPath">The path where unparsable log lines will be written. Existing content will be overwritten.</param>
    /// <remarks>
    /// <para>
    /// This method:
    /// <list type="bullet">
    ///   <item>Validates that the input file exists.</item>
    ///   <item>Processes the file line by line using <see cref="LogTransformationService"/>.</item>
    ///   <item>Writes successfully transformed lines to <paramref name="outputLogPath"/>.</item>
    ///   <item>Writes lines that fail parsing to <paramref name="problemLogPath"/>.</item>
    ///   <item>Reports processing statistics to <see cref="Console.Out"/>.</item>
    ///   <item>Writes errors to <see cref="Console.Error"/> and continues or exits gracefully.</item>
    /// </list>
    /// </para>
    /// <para>
    /// The method is fail-soft for individual lines (unparsable lines go to problems file)
    /// but fail-fast for I/O errors (e.g. missing input file, write permissions).
    /// </para>
    /// </remarks>
    public static void PerformLogTransformation(string inputLogPath, string outputLogPath, string problemLogPath)
    {
        if (!File.Exists(inputLogPath))
        {
            Console.Error.WriteLine($"Input file not found: {inputLogPath}");
            return;
        }

        try
        {
            using var inputStream = new StreamReader(inputLogPath);
            using var outputStream = new StreamWriter(outputLogPath);
            using var problemStream = new StreamWriter(problemLogPath);

            var service = new LogTransformationService();
            string inputLogLine;

            int transformedLogCounter = 0;
            int problemLogCounter = 0;

            while ((inputLogLine = inputStream.ReadLine()) != null)
            {
                try
                {
                    string outputLogLine = service.TransformLine(inputLogLine);
                    outputStream.WriteLine(outputLogLine);
                    transformedLogCounter++;
                }
                catch
                {
                    problemStream.WriteLine(inputLogLine);
                    problemLogCounter++;
                }
            }

            if (transformedLogCounter > 0)
            {
                Console.WriteLine($"Processed {transformedLogCounter} log entries → saved to \"{outputLogPath}\"");
            }
            else
            {
                Console.WriteLine($"No valid log entries found. Output file \"{outputLogPath}\" may be empty.");
            }

            if (problemLogCounter > 0)
            {
                Console.WriteLine($"Skipped {problemLogCounter} invalid log entries → saved to \"{problemLogPath}\"");
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to process logs: {ex.Message}");
        }
    }
}