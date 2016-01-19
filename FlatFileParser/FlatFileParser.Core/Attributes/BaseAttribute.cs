using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlatFileParser.Core.Attributes
{
    public class BaseAttribute : Attribute
    {
        /// <summary>
        /// The name of a public, static method to call on a non-string type that will load the value. Defaults to "Parse".
        /// </summary>
        public string ParseMethod { get; set; }

        /// <summary>
        /// The length of the field.
        /// </summary>
        public int Length { get; set; }
    }
}
