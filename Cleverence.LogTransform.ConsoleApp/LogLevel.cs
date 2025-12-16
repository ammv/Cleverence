using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cleverence.LogTransform.ConsoleApp
{
    /// <summary>
    /// Represents the severity levels of log messages.
    /// </summary>
    public enum LogLevel
    {
        /// <summary>Informational messages.</summary>
        INFO,

        /// <summary>Debug-level messages, typically used for diagnostics.</summary>
        DEBUG,

        /// <summary>Warning messages indicating potential issues.</summary>
        WARN,

        /// <summary>Error messages indicating failures.</summary>
        ERROR
    }
}
