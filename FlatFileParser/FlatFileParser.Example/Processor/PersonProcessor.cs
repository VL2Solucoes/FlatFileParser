using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlatFileParser.Core.Processor;
using FlatFileParser.Example.Entity;

namespace FlatFileParser.Example.Processor
{
    public class PersonProcessor
    {
        public List<Person> Persons { get; set; } = new List<Person>();

        public PersonProcessor(List<string> lines)
        {
            var processor = new FileProcessor<Person>();

            Persons = processor.ProcessLines(lines).ToList();

        }
    }
}
