using System;
using FlatFileParser.Core.Attributes;
using FlatFileParser.Core.Enums;

namespace FlatFileParser.Example.Entity
{
    [ProcessorType(LineProcessingType.Full)]
    public class Person
    {
        [FullProcessorField(1, 5)]
        public int Id { get; set; }

        [FullProcessorField(6, 20)]
        public string Name { get; set; }

        [FullProcessorField(26, 8, "ParseExact", "yyyyMMdd")]
        public DateTime BirthDate { get; set; }

        [FullProcessorField(34, 10, "Parse", "2")]
        public decimal Salary { get; set; }
    }
}
