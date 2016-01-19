using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;

using System.Reflection;
using FlatFileParser.Core.Attributes;
using FlatFileParser.Core.Enums;
using System.Globalization;

namespace FlatFileParser.Core.Processor
{
    public class FileProcessor<T>
    {
        protected List<string> _errorLines = new List<string>();
        /// <summary>
        /// Contains the string contents of each line that fails during processing.
        /// </summary>
        public List<string> ErrorLines
        {
            get { return _errorLines; }
        }

        protected string _newLine = Environment.NewLine;
        /// <summary>
        /// The newline character to use when processing. Defaults to Environment.NewLine.
        /// </summary>
        public string Newline
        {
            get { return _newLine; }
            set { _newLine = value; }
        }

        protected List<ProcessingSetup> _setup = new List<ProcessingSetup>();
        /// <summary>
        /// A list of ProcessingSetup values that allow configuration of how the processor will handle errors.
        /// </summary>
        public List<ProcessingSetup> Setup
        {
            get { return _setup; }
            set { _setup = value; }
        }

        protected LineProcessingType _processingType;
        /// <summary>
        /// The type of processing this processor will conduct, based upon the type which is supplied in the template.
        /// </summary>
        public LineProcessingType ProcessingType
        {
            get { return _processingType; }
        }

        private IEnumerable<ProcessingProperty> _fieldProperties = new List<ProcessingProperty>();

        /// <summary>
        /// Constructor.
        /// </summary>
        public FileProcessor()
        {
            VerifyType();

            // load in properties for later processing
            if (_processingType == LineProcessingType.Full)
            {
                _fieldProperties = from x in typeof(T).GetProperties()
                                   where x.GetCustomAttributes(typeof(FullProcessorField), true).Length != 0
                                   select new ProcessingProperty
                                   {
                                       Name = x.Name,
                                       PropertyType = typeof(T).GetProperty(x.Name).PropertyType,
                                       PropertyAttribute = (Attribute)x.GetCustomAttributes(typeof(FullProcessorField), true).FirstOrDefault()
                                   };

                // sort the properties by their order in the attribute
                _fieldProperties = _fieldProperties.OrderBy(x => ((FullProcessorField)x.PropertyAttribute).Order);
            }
            else
            {
                _fieldProperties = from x in typeof(T).GetProperties()
                                   where x.GetCustomAttributes(typeof(PartialProcessorField), true).Length != 0
                                   select new ProcessingProperty
                                   {
                                       Name = x.Name,
                                       PropertyType = typeof(T).GetProperty(x.Name).PropertyType,
                                       PropertyAttribute = (Attribute)x.GetCustomAttributes(typeof(PartialProcessorField), true).FirstOrDefault()
                                   };
            }

            foreach (var prop in _fieldProperties.Where(x => x.PropertyType != typeof(string)))
            {
                string parseMethod = ((BaseAttribute)prop.PropertyAttribute).ParseMethod;

                //if (prop.PropertyType.GetMethods().Where(x => x.IsPublic && x.IsStatic && x.Name == parseMethod && x.GetParameters().Count() == 1 && x.GetParameters()[0].ParameterType == typeof(System.String)).FirstOrDefault() == null)
                //{
                //    throw new FlatFileParserException("The type '" + prop.PropertyType.Name + "' is a non-string type and it is missing a public static '" + parseMethod + "' method that accepts a single string as a parameter.", FlatFileParserExceptionType.FieldHasNoParseMethod);
                //}

                if (prop.PropertyType.GetMethods().Where(x => x.IsPublic && x.Name == parseMethod && x.GetParameters()[0].ParameterType == typeof(System.String)).FirstOrDefault() == null)
                {
                    throw new FlatFileParserException("The type '" + prop.PropertyType.Name + "' is a non-string type and it is missing a public static '" + parseMethod + "' method that accepts a single string as a parameter.", FlatFileParserExceptionType.FieldHasNoParseMethod);
                }
            }
        }

        /// <summary>
        /// Verifies that the type used for the processor is a valid type.
        /// </summary>
        private void VerifyType()
        {
            // verify the supplied type is configured to parse a full file or a partial file
            if (typeof(T).GetCustomAttributes(typeof(ProcessorType), true).Length == 0)
            {
                throw new FlatFileParserException("The type '" + typeof(T).Name + "' is missing a ProcessorType attribute.", FlatFileParserExceptionType.MissingProcessorTypeAttribute);
            }
            else if (typeof(T).GetCustomAttributes(typeof(ProcessorType), true).Length > 1)
            {
                throw new FlatFileParserException("The type '" + typeof(T).Name + "' has multiple ProcessorType attributes.", FlatFileParserExceptionType.MultipleProcessorTypeAttributes);
            }

            _processingType = ((ProcessorType)typeof(T).GetCustomAttributes(typeof(ProcessorType), true).First()).ProcessingType;

            // verify the supplied object does not contain mixes of attributes (full and partial)
            int numFullFieldAttributes = typeof(T).GetProperties().Select(x => x.GetCustomAttributes(typeof(FullProcessorField), true)).Where(x => x.Count() != 0).Count();
            int numPartialFieldAttributes = typeof(T).GetProperties().Select(x => x.GetCustomAttributes(typeof(PartialProcessorField), true)).Where(x => x.Count() != 0).Count();

            if (numFullFieldAttributes == 0 && numPartialFieldAttributes == 0)
            {
                throw new FlatFileParserException("The type '" + typeof(T).Name + "' does not define any full or partial fields.", FlatFileParserExceptionType.NoProcessingFieldsDefined);
            }
            else if (_processingType == LineProcessingType.Full && numPartialFieldAttributes != 0)
            {
                throw new FlatFileParserException("The type '" + typeof(T).Name + "' has a Full ProcessorType attribute but has properties defined that are Partial fields.", FlatFileParserExceptionType.ProcessorTypeFieldMismatch);
            }
            else if (_processingType == LineProcessingType.Partial && numFullFieldAttributes != 0)
            {
                throw new FlatFileParserException("The type '" + typeof(T).Name + "' has a Partial ProcessorType attribute but has properties defined that are Full fields.", FlatFileParserExceptionType.ProcessorTypeFieldMismatch);
            }

            // if this is a writable type, check that all the properties have a "ToString()" or a proper WriteAction.
            if (typeof(T).GetCustomAttributes(typeof(WritableType), false).Length != 0)
            {

            }
        }

        /// <summary>
        /// Reads in a file and splits it based upon NewLine.
        /// </summary>
        /// <param name="path">The path to read in.</param>
        /// <returns>An enumerable object containing the strings that were split out of the file.</returns>
        private IEnumerable<string> ReadFile(string path)
        {
            string file;

            if (File.Exists(path) == false)
            {
                throw new FlatFileParserException("File '" + path + "' does not exist.", FlatFileParserExceptionType.FileDoesNotExist);
            }

            StreamReader sw = new StreamReader(path);
            file = sw.ReadToEnd();
            sw.Close();

            if (_setup.Contains(ProcessingSetup.SkipEmptyLines))
            {
                return file.Split(new string[] { _newLine }, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                return file.Split(new string[] { _newLine }, StringSplitOptions.None);
            }
        }

        /// <summary>
        /// Returns true if the supplied setup value has been configured.
        /// </summary>
        /// <param name="setupValue">The value to check for.</param>
        /// <returns>True if the setup value is configured, false otherwise.</returns>
        private bool CheckSetupValue(ProcessingSetup setupValue)
        {
            return _setup.Contains(setupValue);
        }

        /// <summary>
        /// Instanciates a new object of type T.
        /// </summary>
        /// <returns>A new T object.</returns>
        private T NewInstance()
        {
            return (T)System.Activator.CreateInstance(typeof(T));
        }

        /// <summary>
        /// Pulls out the text of the line supplied for the given start and length. Checks for errors.
        /// </summary>
        /// <param name="line">The line to process.</param>
        /// <param name="lineNumber">The current line number.</param>
        /// <param name="start">The starting position to pull text from.</param>
        /// <param name="length">The length of substring to pull.</param>
        /// <returns>A string represent the value in line between start point and start point + length.</returns>
        private string GetStringFromLine(string line, int lineNumber, int start, int length)
        {
            if (start + length > line.Length)
            {
                throw new FlatFileParserException("Error, line # " + lineNumber.ToString() + ". Attempted to read past end of line.", FlatFileParserExceptionType.ReadPastEndOfLine);
            }

            return line.Substring(start, length);
        }

        /// <summary>
        /// Sets a property in the T object based upon the text provided.
        /// </summary>
        /// <param name="obj">The object to set the property in.</param>
        /// <param name="property">The property to set.</param>
        /// <param name="text">The text in which to pull the value to set.</param>
        /// <param name="lineNumber">The current line number.</param>
        private void SetProperty(T obj, ProcessingProperty property, string parseCommand, string format, string text, int lineNumber)
        {
            if (property.PropertyType == typeof(string) && parseCommand == "Parse")
            {
                typeof(T).GetProperty(property.Name).SetValue(obj, text, null);
            }
            else if (property.PropertyType == typeof(DateTime) && parseCommand == "ParseExact")
            {
                typeof(T).GetProperty(property.Name).SetValue(obj, property.PropertyType.InvokeMember(parseCommand, BindingFlags.InvokeMethod, null, null, new object[] { text, format, CultureInfo.CurrentCulture }), null);
            }
            else if (property.PropertyType == typeof(decimal) && parseCommand == "Parse")
            {
                var value = (decimal)property.PropertyType.InvokeMember(parseCommand, BindingFlags.InvokeMethod, null, null, new object[] { text });
                var decimalPlaces = 10 ^ Convert.ToInt32(format);

                typeof(T).GetProperty(property.Name).SetValue(obj, value / decimalPlaces , null);
            }
            else
            {
                try
                {
                    typeof(T).GetProperty(property.Name).SetValue(obj, property.PropertyType.InvokeMember(parseCommand, BindingFlags.InvokeMethod, null, null, new object[] { text }), null);
                }
                catch (Exception e)
                {
                    throw new FlatFileParserException("Error, line # " + lineNumber + ". Call to '" + parseCommand + "' failed on field '" + property.Name + "' with value '" + text + "'", FlatFileParserExceptionType.ParseFailed);
                }

            }
        }

        /// <summary>
        /// Parses a line using the "full" methodology which reads data from an entire line.
        /// </summary>
        /// <param name="line">The text to process.</param>
        /// <param name="lineNumber">The current line number.</param>
        /// <returns>An object of type T which is the result of processing.</returns>
        private T ParseLineFull(string line, int lineNumber)
        {
            var value = NewInstance();

            int expectedLength = _fieldProperties.Sum(x => (((FullProcessorField)x.PropertyAttribute).Length));

            if (line.Length != expectedLength)
            {
                throw new FlatFileParserException("Error, line # " + lineNumber.ToString() + ". The line contains " + line.Length.ToString() + " characters and expected " + expectedLength.ToString(), FlatFileParserExceptionType.LineLengthDoesNotMatch);
            }

            int currentPosition = 0;
            foreach (var p in _fieldProperties)
            {
                FullProcessorField field = (FullProcessorField)p.PropertyAttribute;

                SetProperty(value, p, field.ParseMethod, field.FormatString, GetStringFromLine(line, lineNumber, currentPosition, field.Length), lineNumber);

                currentPosition += field.Length;
            }

            return value;
        }

        /// <summary>
        /// Parses a line using the "partial" methodology which reads data from parts of a line.
        /// </summary>
        /// <param name="line">The line to process.</param>
        /// <param name="lineNumber">The current line number.</param>
        /// <returns>An object of type T which represents the results of processing.</returns>
        private T ParseLinePartial(string line, int lineNumber)
        {
            var value = NewInstance();

            foreach (var p in _fieldProperties)
            {
                PartialProcessorField field = (PartialProcessorField)p.PropertyAttribute;

                SetProperty(value, p, field.ParseMethod, null, GetStringFromLine(line, lineNumber, field.Start, field.Length), lineNumber);
            }

            return value;
        }

        /// <summary>
        /// Processes and entire line using the correct methodology (full or partial.)
        /// </summary>
        /// <param name="line">The line to process.</param>
        /// <param name="lineNumber">The current line number.</param>
        /// <returns>An object of type T which represents the result of the processing.</returns>
        private T ProcessLine(string line, int lineNumber)
        {
            if (string.IsNullOrEmpty(line))
            {
                throw new FlatFileParserException("Error, Line # " + lineNumber + ". Attempting to parse empty line.", FlatFileParserExceptionType.LineEmpty);
            }

            T value = default(T);

            if (_processingType == LineProcessingType.Partial)
            {
                value = ParseLinePartial(line, lineNumber);
            }
            else
            {
                value = ParseLineFull(line, lineNumber);
            }

            return value;
        }

        /// <summary>
        /// Either ignores the error based upon the configuration, or throws the error further down the line.
        /// </summary>
        /// <param name="erroredLine">The line that was being processed.</param>
        /// <param name="e">The exception that occured.</param>
        private void HandleError(string erroredLine, FlatFileParserException e)
        {
            bool throwException = true;
            _errorLines.Add(erroredLine);

            if (CheckSetupValue(ProcessingSetup.SkipLineOnFailure) ||
                (CheckSetupValue(ProcessingSetup.SkipEmptyLines) && e.Type == FlatFileParserExceptionType.LineEmpty))
            {
                throwException = false;
            }

            if (throwException)
            {
                throw (e);
            }
        }

        /// <summary>
        /// Reads in a file from disk and processes each line into a list of objects of type T.
        /// </summary>
        /// <param name="path">The file to read.</param>
        /// <returns>An enumerable result of objects representing the data in the file.</returns>
        public IEnumerable<T> ProcessFile(string path)
        {
            List<T> results = new List<T>();

            ErrorLines.Clear();

            int lineNumber = 1;
            foreach (var s in ReadFile(path))
            {
                try
                {
                    results.Add(ProcessLine(s, lineNumber));
                }
                catch (FlatFileParserException e)
                {
                    HandleError(s, e);
                }

                lineNumber++;
            }

            return results.AsEnumerable();
        }

        /// <summary>
        /// Processes lines in a given string into a list of objects of type T.
        /// </summary>
        /// <param name="text">The text to process.</param>
        /// <returns>An enumerable result of objects representing the data in the file.</returns>
        public IEnumerable<T> ProcessString(string text)
        {
            List<T> results = new List<T>();
            IEnumerable<string> lines;

            if (CheckSetupValue(ProcessingSetup.SkipEmptyLines))
            {
                lines = text.Split(new string[] { _newLine }, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                lines = text.Split(new string[] { _newLine }, StringSplitOptions.None);
            }

            ErrorLines.Clear();

            int lineNumber = 1;
            foreach (var s in lines)
            {
                try
                {
                    results.Add(ProcessLine(s, lineNumber));
                }
                catch (FlatFileParserException e)
                {
                    HandleError(s, e);
                }

                lineNumber++;
            }

            return results.AsEnumerable();
        }

        public IEnumerable<T> ProcessLines(IEnumerable<string> lines)
        {
            List<T> results = new List<T>();

            ErrorLines.Clear();

            int lineNumber = 1;
            foreach (var s in lines)
            {
                try
                {
                    results.Add(ProcessLine(s, lineNumber));
                }
                catch (FlatFileParserException e)
                {
                    HandleError(s, e);
                }

                lineNumber++;
            }

            return results.AsEnumerable();
        }

        private string GetItemString(T item)
        {
            string value = "";

            if (_processingType == LineProcessingType.Full)
            {
                foreach (var field in _fieldProperties)
                {
                    FullProcessorField f = (FullProcessorField)field.PropertyAttribute;
                    TypeCode typeCode = Type.GetTypeCode(field.PropertyType);
                    bool padLeft = false;

                    if (typeCode == TypeCode.Byte || typeCode == TypeCode.Decimal || typeCode == TypeCode.Double || typeCode == TypeCode.Int16
                        || typeCode == TypeCode.Int32 || typeCode == TypeCode.Int64 || typeCode == TypeCode.SByte || typeCode == TypeCode.Single
                        || typeCode == TypeCode.UInt16 || typeCode == TypeCode.UInt32 || typeCode == TypeCode.UInt64)
                    {
                        padLeft = true;
                    }

                    string text = typeof(T).GetProperty(field.Name).GetValue(item, BindingFlags.GetProperty, null, null, System.Globalization.CultureInfo.CurrentCulture).ToString();

                    if (padLeft)
                    {
                        text = text.PadLeft(f.Length, '0');
                    }
                    else
                    {
                        text = text.PadRight(f.Length, ' ');
                    }

                    value += text;
                }
            }
            else
            {
                var lastValue = (PartialProcessorField)_fieldProperties.Select(x => x.PropertyAttribute).OrderByDescending(x => ((PartialProcessorField)x).Start).First();
                value = value.PadLeft(lastValue.Start + lastValue.Length, ' ');

                foreach (var field in _fieldProperties)
                {
                    PartialProcessorField f = (PartialProcessorField)field.PropertyAttribute;
                    bool padLeft = false;
                    TypeCode typeCode = Type.GetTypeCode(field.PropertyType);

                    if (typeCode == TypeCode.Byte || typeCode == TypeCode.Decimal || typeCode == TypeCode.Double || typeCode == TypeCode.Int16
                        || typeCode == TypeCode.Int32 || typeCode == TypeCode.Int64 || typeCode == TypeCode.SByte || typeCode == TypeCode.Single
                        || typeCode == TypeCode.UInt16 || typeCode == TypeCode.UInt32 || typeCode == TypeCode.UInt64)
                    {
                        padLeft = true;
                    }

                    string text = typeof(T).GetProperty(field.Name).GetValue(item, BindingFlags.GetProperty, null, null, System.Globalization.CultureInfo.CurrentCulture).ToString();

                    if (padLeft)
                    {
                        text = text.PadLeft(f.Length, '0');
                    }
                    else
                    {
                        text = text.PadRight(f.Length, ' ');
                    }

                    value = value.Substring(0, f.Start) + text + value.Substring(f.Start);
                }
            }

            return value;
        }

        /// <summary>
        /// Writes the supplied items to the given path in a flat file format which matches the same format which gets loaded in.
        /// </summary>
        /// <param name="items">The items to write.</param>
        /// <param name="path">The path of the file to save.</param>
        //public void WriteToFile(IEnumerable<T> items, string path)
        //{
        //    foreach (var item in items)
        //    {
        //        GetItemString
        //    }

        //    var writeAttribute = (WritableType)typeof(T).GetCustomAttributes(typeof(WritableType), true).FirstOrDefault();

        //    if (writeAttribute == null)
        //    {
        //        throw new FlatFileParserException("Calling WriteToFile on a type '" + typeof(T).Name + "' which does not have a WritableType attribute.", FlatFileParserExceptionType.MissingToStringMethod);
        //    }

        //    StreamWriter sw = new StreamWriter(path, false);

        //    foreach (var item in items)
        //    {
        //        sw.Write(GetItemString(item) + _newLine);
        //        //sw.Write((string)typeof(T).InvokeMember(writeAttribute.WriteMethod, BindingFlags.InvokeMethod, null, item, null) + _newLine);
        //    }

        //    sw.Close();
        //}
    }
}
