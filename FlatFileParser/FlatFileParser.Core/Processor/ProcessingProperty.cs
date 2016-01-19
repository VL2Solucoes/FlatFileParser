using System;
using System.Collections.Generic;
using System.Text;

namespace FlatFileParser.Core.Processor
{
    /// <summary>
    /// A container of a property within the target class loaded by the processor. It contains additional information to help process the file.
    /// </summary>
    internal class ProcessingProperty
    {
        /// <summary>
        /// The name of the property to set in the target class.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The type of the property to set in the target class.
        /// </summary>
        public Type PropertyType { get; set; }

        /// <summary>
        /// The attribute which is set on the target property in the target class.
        /// </summary>
        public Attribute PropertyAttribute { get; set; }
    }
}
