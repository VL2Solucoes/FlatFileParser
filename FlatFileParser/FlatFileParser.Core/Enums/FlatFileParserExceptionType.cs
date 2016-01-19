using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlatFileParser.Core.Enums
{
    /// <summary>
    /// Describes the type of exception that is raised by the processor.
    /// </summary>
    public enum FlatFileParserExceptionType
    {
        /// <summary>
        /// The type that is passed in as the template type to the processor does not have a ProcessorType attribute.
        /// </summary>
        MissingProcessorTypeAttribute,

        /// <summary>
        /// The type that is passed in as the template type to the processor has multiple ProcessorType attributes.
        /// </summary>
        MultipleProcessorTypeAttributes,

        /// <summary>
        /// No fields exist in the template type that have a FullProcessorField attribute or a PartialProcessorField attribute.
        /// </summary>
        NoProcessingFieldsDefined,
        
        /// <summary>
        /// A PartialProcessorField exists in the template type and the type is a Full Processing Type, or a FullProcessorField exists
        /// in the template and the type is a Partial Processing Type.
        /// </summary>
        ProcessorTypeFieldMismatch,

        /// <summary>
        /// A field which is not of type string has been found in the template type which does not have a static Parse method which accepts
        /// a single string as input.
        /// </summary>
        FieldHasNoParseMethod,

        /// <summary>
        /// The file which is being processed does not exist.
        /// </summary>
        FileDoesNotExist,

        /// <summary>
        /// Attempted to read past the end of a line while processing a partial field.
        /// </summary>
        ReadPastEndOfLine,

        /// <summary>
        /// The line which is being processed in a full method does not match the expected length of the line summed up in the FullProcessorFields.
        /// </summary>
        LineLengthDoesNotMatch,

        /// <summary>
        /// The line which is being processed is empty.
        /// </summary>
        LineEmpty,

        /// <summary>
        /// The line which is being parsed failed to process.
        /// </summary>
        ParseFailed,

        /// <summary>
        /// A writable type is missing a ToString() method.
        /// </summary>
        MissingToStringMethod
    }
}
