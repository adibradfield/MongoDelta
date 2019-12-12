using System;
using System.Collections.Generic;
using MongoDB.Driver;
using MongoDelta.Mapping;

namespace MongoDelta.IntegrationTests.Models
{
    [UseDeltaUpdateStrategy]
    class CollectionUpdateAggregate
    {
        public Guid Id { get; set; }

        [UpdateAsHashSet]
        public HashSet<string> HashSet { get; set; } = new HashSet<string>();

        [UpdateAsDeltaSet(typeof(AddressAggregate))] 
        public List<AddressAggregate> DeltaSet { get; set; } = new List<AddressAggregate>();
    }

    class CollectionUnitOfWork : UnitOfWorkBase
    {
        public CollectionUnitOfWork(IMongoDatabase database, string collectionName) : base(database, false)
        {
            RegisterRepository<CollectionUpdateAggregate>(collectionName);
        }

        public MongoDeltaRepository<CollectionUpdateAggregate> Collections => GetRepository<CollectionUpdateAggregate>();
    }
}
