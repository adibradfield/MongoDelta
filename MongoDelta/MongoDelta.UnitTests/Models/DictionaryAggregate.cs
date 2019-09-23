using System;
using System.Collections.Generic;

namespace MongoDelta.UnitTests.Models
{
    public class DictionaryAggregate
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Dictionary<string, PersonAge> Ages { get; set; } = new Dictionary<string, PersonAge>()
        {
            { "Bob", new PersonAge(20) },
            { "John", new PersonAge(44) }
        };

        public class PersonAge
        {
            public int Age { get; set; }

            public PersonAge(int age)
            {
                Age = age;
            }
        }
    }
}
