using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlatFileParser.Core.Attributes
{
    public class FormatString : BaseAttribute
    {
        public string Format { get; set; }

        public FormatString(string format)
        {
            Format = format;
        }
    }
}
