using System;
using MongoDB.Driver;
using MongoDelta.Mapping;

namespace MongoDelta.IntegrationTests.Models
{
    [UseDeltaUpdateStrategy]
    public class AddressAggregate
    {
        public Guid Id { get; set; }
        public string HouseNumber { get; set; }
        public string HouseName { get; set; }
        public string Street { get; set; }
        public string Town { get; set; }
        public string Postcode { get; set; }
    }

    public class AddressUnitOfWork : UnitOfWorkBase
    {
        public AddressUnitOfWork(IMongoDatabase database, string collectionName) : base(database, false)
        {
            RegisterRepository<AddressAggregate>(collectionName);
        }

        public MongoDeltaRepository<AddressAggregate> Addresses => GetRepository<AddressAggregate>();
    }
}
