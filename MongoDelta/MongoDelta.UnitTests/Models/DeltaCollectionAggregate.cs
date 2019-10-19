using System;
using System.Collections.Generic;
using MongoDelta.Mapping;

namespace MongoDelta.UnitTests.Models
{
    [UseDeltaUpdateStrategy]
    class DeltaCollectionAggregate
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [UpdateAsHashSet]
        public List<string> HashSet { get; set; } = new List<string>();
    }
}
