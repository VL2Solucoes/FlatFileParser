using System;
using System.Collections.Generic;
using System.Text;

namespace FlatFileParser.Core.Attributes
{
    public class FullProcessorField : BaseAttribute
    {
        /// <summary>
        /// The serial order of this field in the file.
        /// </summary>
        public int Order { get; set; }

        public string FormatString { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="order">The serial order of this field in the file.</param>
        /// <param name="length">The length of the field.</param>
        public FullProcessorField(int order, int length)
        {
            Order = order;
            Length = length;
            ParseMethod = "Parse";
            FormatString = "0";
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="order">The serial order of this field in the file.</param>
        /// <param name="length">The length of the field.</param>
        /// <param name="parseMethod">The name of a public, static method to call on a non-string type which will load its value from a string. Example: Parse</param>
        public FullProcessorField(int order, int length, string parseMethod)
        {
            Order = order;
            Length = length;
            ParseMethod = parseMethod;
            FormatString = "0";
        }

        public FullProcessorField(int order, int length, string parseMethod, string formatString)
        {
            Order = order;
            Length = length;
            ParseMethod = parseMethod;
            FormatString = formatString;
        }
    }
}
