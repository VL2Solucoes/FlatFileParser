using System;
using System.Collections.Generic;
using System.Text;

namespace FlatFileParser.Core.Enums
{
    /// <summary>
    /// The type of processing to conduct on a line in the file.
    /// </summary>
    public enum LineProcessingType
    {
        /// <summary>
        /// Process the entire line.
        /// </summary>
        Full,

        /// <summary>
        /// Process pieces of the line at a time.
        /// </summary>
        Partial
    }
}
