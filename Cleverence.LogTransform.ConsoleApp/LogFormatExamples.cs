namespace Cleverence.LogTransform.ConsoleApp
{
    /// <summary>
    /// Provides example log strings for supported input and output formats.
    /// Used for documentation, help messages, and testing.
    /// </summary>
    public static class LogFormatExamples
    {
        /// <summary>
        /// Example of input log format 1: space-delimited with date, time, level, and message.
        /// </summary>
        public const string InputLogFormat1Example = "10.03.2025 15:14:49.523 INFORMATION Program version: '3.4.0.48729'";

        /// <summary>
        /// Example of input log format 2: pipe-delimited with datetime, level, thread ID, method, and message.
        /// </summary>
        public const string InputLogFormat2Example = "2025-03-10 15:14:51.5882| INFO|11|MobileComputer.GetDeviceId| Device id: '@MINDEO-M40-D-410244015546'";

        /// <summary>
        /// Example of unified output format corresponding to input format 1.
        /// Tab-separated with normalized date, time, level, "DEFAULT" method, and message.
        /// </summary>
        public const string OutputLogFormatForInputLogFormat1Example = "10-03-2025\t15:14:49.523\tINFO\tDEFAULT\tProgram version: '3.4.0.48729'";

        /// <summary>
        /// Example of unified output format corresponding to input format 2.
        /// Tab-separated with normalized date, time, level, original method, and message.
        /// </summary>
        public const string OutputLogFormatForInputLogFormat2Example = "10-03-2025\t15:14:51.5882\tINFO\tMobileComputer.GetDeviceId\tDevice id: '@MINDEO-M40-D-410244015546'";
    }
}