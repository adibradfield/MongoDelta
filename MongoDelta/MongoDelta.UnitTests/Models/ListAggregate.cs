using System;
using System.Collections.Generic;

namespace MongoDelta.UnitTests.Models
{
    public class ListAggregate
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public List<PersonName> Names { get; set; } = new List<PersonName>()
        {
            new PersonName("Bob"),
            new PersonName("John")
        };

        public class PersonName
        {
            public string Name { get; set; }

            public PersonName(string name)
            {
                Name = name;
            }
        }
    }
}
