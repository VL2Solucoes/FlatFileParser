using System;
using System.Collections.Generic;
using System.Text;

namespace FlatFileParser.Core.Attributes
{
    public class PartialProcessorField : BaseAttribute
    {
        /// <summary>
        /// The position in the line to start pulling data.
        /// </summary>
        public int Start { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="start">The position in the line to start pulling data.</param>
        /// <param name="length">The length of the amount of text to pull.</param>
        public PartialProcessorField(int start, int length)
        {
            Start = start;
            Length = length;
            ParseMethod = "Parse";
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="start">The position in the line to start pulling data.</param>
        /// <param name="length">The length of the  amount of text to pull.</param>
        /// <param name="parseMethod">The name of a public, static method to call on a non-string type which will load its value from a string. Example: Parse</param>
        public PartialProcessorField(int start, int length, string parseMethod)
        {
            Start = start;
            Length = length;
            ParseMethod = parseMethod;
        }
    }
}
