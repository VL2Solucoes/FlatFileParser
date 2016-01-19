using System;
using System.Collections.Generic;
using System.Text;

namespace FlatFileParser.Core.Enums
{
    /// <summary>
    /// Allows configuration of how the processor will handle errors.
    /// </summary>
    public enum ProcessingSetup
    {
        /// <summary>
        /// Instructs the processor to continue if a line fails processing. The line will remain in the ErrorLines list of errors.
        /// </summary>
        SkipLineOnFailure,

        /// <summary>
        /// Instructs the processor to skip empty lines. The line will remain in the ErrorLines list of errors.
        /// </summary>
        SkipEmptyLines
    }
}
