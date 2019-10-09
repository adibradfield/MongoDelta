using System;
using MongoDB.Driver;
using MongoDelta.Mapping;

namespace MongoDelta.IntegrationTests.Models
{
    [UseDeltaUpdateStrategy]
    class CustomerAggregate
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public CustomerDefaultAddresses DefaultAddresses { get; set; }
    }

    [UseDeltaUpdateStrategy]
    class CustomerDefaultAddresses
    {
        public AddressAggregate InvoiceAddress { get; set; }
        public AddressAggregate ShippingAddress { get; set; }
    }

    class CustomerUnitOfWork : UnitOfWorkBase
    {
        public CustomerUnitOfWork(IMongoDatabase database, string collectionName) : base(database, false)
        {
            RegisterRepository<CustomerAggregate>(collectionName);
        }

        public MongoDeltaRepository<CustomerAggregate> Customers => GetRepository<CustomerAggregate>();
    }
}
