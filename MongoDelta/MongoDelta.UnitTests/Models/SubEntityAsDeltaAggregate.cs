using System;
using MongoDelta.Mapping;

namespace MongoDelta.UnitTests.Models
{
    [UseDeltaUpdateStrategy]
    public class SubEntityAsDeltaAggregate
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DeltaSubEntity DeltaValue { get; set; }
        public NonDeltaSubEntity NonDeltaValue { get; set; }

        [UseDeltaUpdateStrategy]
        public class DeltaSubEntity
        {
            public string Value { get; set; }
        }

        public class NonDeltaSubEntity
        {
            public string Value { get; set; }
        }
    }
}
