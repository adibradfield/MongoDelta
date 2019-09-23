using System;

namespace MongoDelta.UnitTests.Models
{
    public class FlatAggregate
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "John Smith";
        public int Age { get; set; } = 20;
        public DateTime DateOfBirth { get; set; } = new DateTime(1995, 01, 01);
    }
}
