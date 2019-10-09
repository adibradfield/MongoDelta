using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDelta.Mapping;

namespace MongoDelta.IntegrationTests.Models
{
    [UseDeltaUpdateStrategy]
    class IncrementalUpdateAggregate
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "OriginalName";

        [UpdateIncrementally]
        public int Integer { get; set; }

        [UpdateIncrementally]
        public long Long { get; set; }

        [UpdateIncrementally]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Decimal { get; set; }
    }

    class IncrementalUpdateUnitOfWork : UnitOfWorkBase
    {
        public IncrementalUpdateUnitOfWork(IMongoDatabase database, string collectionName) : base(database, false)
        {
            RegisterRepository<IncrementalUpdateAggregate>(collectionName);
        }

        public MongoDeltaRepository<IncrementalUpdateAggregate> Models => GetRepository<IncrementalUpdateAggregate>();
    }
}
