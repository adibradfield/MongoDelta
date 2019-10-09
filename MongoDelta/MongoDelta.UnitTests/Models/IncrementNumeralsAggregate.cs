using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDelta.Mapping;

namespace MongoDelta.UnitTests.Models
{
    [UseDeltaUpdateStrategy]
    public class IncrementNumeralsAggregate
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }

        [UpdateIncrementally]
        public int Integer { get; set; }

        [UpdateIncrementally]
        public long Long { get; set; }

        [UpdateIncrementally]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Decimal { get; set; }

        [UpdateIncrementally]
        public double Double { get; set; }
    }
}
