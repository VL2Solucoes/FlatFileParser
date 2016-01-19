using System;
using System.Collections.Generic;
using System.Text;

using FlatFileParser.Core.Enums;

namespace FlatFileParser.Core.Attributes
{
    /// <summary>
    /// An attribute which will determine how the processor processes files. Full processing will parse the entire line in a file, while partial will parse only portions of the line.
    /// </summary>
    public class ProcessorType : Attribute
    {
        /// <summary>
        /// The type of processing the processor will operate with.
        /// </summary>
        public LineProcessingType ProcessingType { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="type">Either full processing or partial processing.</param>
        public ProcessorType(LineProcessingType type)
        {
            ProcessingType = type;
        }
    }
}
