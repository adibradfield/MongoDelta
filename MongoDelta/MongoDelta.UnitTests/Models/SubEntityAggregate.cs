using System;

namespace MongoDelta.UnitTests.Models
{
    public class SubEntityAggregate
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public SubEntity Entity { get; set; } = new SubEntity();

        public class SubEntity
        {
            public string Name { get; set; } = "John Smith";
        }
    }
}
