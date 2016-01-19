using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FlatFileParser.Core.Enums;

namespace FlatFileParser.Core.Processor
{
    /// <summary>
    /// An exception occuring within the processor.
    /// </summary>
    public class FlatFileParserException : Exception
    {
        /// <summary>
        /// The type of the exception.
        /// </summary>
        public FlatFileParserExceptionType Type { get; set; }

        public FlatFileParserException(string message, FlatFileParserExceptionType type)
            : base(message)
        {
            Type = type;
        }
    }
}
