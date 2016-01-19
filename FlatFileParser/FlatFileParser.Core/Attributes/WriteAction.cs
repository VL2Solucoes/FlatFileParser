using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlatFileParser.Core.Attributes
{
    public class WriteAction : Attribute
    {
        public string Action { get; set; }

        public WriteAction(string action)
        {
            Action = action;
        }
    }
}
