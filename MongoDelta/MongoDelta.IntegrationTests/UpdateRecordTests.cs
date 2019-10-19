using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDelta.IntegrationTests.Models;
using NUnit.Framework;

namespace MongoDelta.IntegrationTests
{
    [TestFixture]
    public class UpdateRecordTests : MongoTestBase
    {
        [Test]
        public async Task AddAndUpdate_SimpleObject_Success()
        {
            var testUser = await UserAggregate.AddTestUser(Database, CollectionName);

            var unitOfWork = new UserUnitOfWork(Database, CollectionName);
            var model = await unitOfWork.Users.QuerySingleAsync(user => user.Id == testUser.Id);

            model.FirstName = "Bobby";
            await unitOfWork.CommitAsync();

            var updateQuery = await unitOfWork.Users.QuerySingleAsync(user => user.FirstName == "Bobby");
            Assert.IsNotNull(updateQuery);
        }

        [Test]
        public async Task UpdateSingleField_WithDeltaUpdateStrategy_Success()
        {
            var initialAddressInstance = new AddressAggregate
            {
                HouseNumber = "40",
                Street = "Grove Street",
                Town = "Manchester",
                Postcode = "M1 1AA"
            };

            var initialUnitOfWork = new AddressUnitOfWork(Database, CollectionName);
            initialUnitOfWork.Addresses.Add(initialAddressInstance);
            await initialUnitOfWork.CommitAsync();

            var outerUpdateUnitOfWork = new AddressUnitOfWork(Database, CollectionName);
            var innerUpdateUnitOfWork = new AddressUnitOfWork(Database, CollectionName);
            var outerUpdateInstance =
                await outerUpdateUnitOfWork.Addresses.QuerySingleAsync(a => a.Id == initialAddressInstance.Id);
            var innerUpdateInstance =
                await outerUpdateUnitOfWork.Addresses.QuerySingleAsync(a => a.Id == initialAddressInstance.Id);

            innerUpdateInstance.HouseNumber = "30";
            await innerUpdateUnitOfWork.CommitAsync();

            outerUpdateInstance.Street = "Wood Avenue";
            await outerUpdateUnitOfWork.CommitAsync();

            var result = await initialUnitOfWork.Addresses.QuerySingleAsync(a => a.Id == initialAddressInstance.Id);
            Assert.AreEqual("30", result.HouseNumber);
            Assert.AreEqual("Wood Avenue", result.Street);
        }

        [Test]
        public async Task AddNewSubObject_WithDeltaUpdateStrategy_Success()
        {
            var createdCustomer = await CreateExistingCustomerAggregate();

            var updateUnitOfWork = new CustomerUnitOfWork(Database, CollectionName);
            var modelToUpdate = await updateUnitOfWork.Customers.QuerySingleAsync(c => c.Id == createdCustomer.Id);
            modelToUpdate.DefaultAddresses.ShippingAddress = new AddressAggregate
            {
                HouseName = "Shipping Address"
            };
            await updateUnitOfWork.CommitAsync();

            var updatedCustomer = await updateUnitOfWork.Customers.QuerySingleAsync(c => c.Id == createdCustomer.Id);
            Assert.AreEqual("Shipping Address", updatedCustomer.DefaultAddresses.ShippingAddress.HouseName);
        }

        [Test]
        public async Task RemoveSubObject_WithDeltaUpdateStrategy_Success()
        {
            var createdCustomer = await CreateExistingCustomerAggregate();

            var updateUnitOfWork = new CustomerUnitOfWork(Database, CollectionName);
            var modelToUpdate = await updateUnitOfWork.Customers.QuerySingleAsync(c => c.Id == createdCustomer.Id);
            modelToUpdate.DefaultAddresses = null;
            await updateUnitOfWork.CommitAsync();

            var updatedCustomer = await updateUnitOfWork.Customers.QuerySingleAsync(c => c.Id == createdCustomer.Id);
            Assert.IsNull(updatedCustomer.DefaultAddresses);
        }

        [Test]
        public async Task UpdateFieldOfSubObject_WithDeltaUpdateStrategy_Success()
        {
            var createdCustomer = await CreateExistingCustomerAggregate();

            var updateUnitOfWork = new CustomerUnitOfWork(Database, CollectionName);
            var modelToUpdate = await updateUnitOfWork.Customers.QuerySingleAsync(c => c.Id == createdCustomer.Id);
            modelToUpdate.DefaultAddresses.InvoiceAddress.HouseName = "New Address";
            await updateUnitOfWork.CommitAsync();

            var updatedCustomer = await updateUnitOfWork.Customers.QuerySingleAsync(c => c.Id == createdCustomer.Id);
            Assert.AreEqual("New Address", updatedCustomer.DefaultAddresses.InvoiceAddress.HouseName);
        }

        [Test]
        public async Task UpdateFieldsIncrementally_Success()
        {
            var createdModel = new IncrementalUpdateAggregate();
            var createUnitOfWork =  new IncrementalUpdateUnitOfWork(Database, CollectionName);
            createUnitOfWork.Models.Add(createdModel);
            await createUnitOfWork.CommitAsync();

            var updateUnitOfWork = new IncrementalUpdateUnitOfWork(Database, CollectionName);
            var updateModel = await updateUnitOfWork.Models.QuerySingleAsync(m => m.Id == createdModel.Id);
            updateModel.Integer += 5;
            updateModel.Decimal += 10.2M;
            updateModel.Long += 20;
            updateModel.Name = "NewName";
            await updateUnitOfWork.CommitAsync();

            var updatedModel = await updateUnitOfWork.Models.QuerySingleAsync(m => m.Id == createdModel.Id);
            Assert.AreEqual(updateModel.Integer, updatedModel.Integer);
            Assert.AreEqual(updateModel.Decimal, updatedModel.Decimal);
            Assert.AreEqual(updateModel.Long, updatedModel.Long);
            Assert.AreEqual("NewName", updatedModel.Name);
        }

        [Test]
        public async Task UpdateAsHashSet_Success()
        {
            var createdModel = new CollectionUpdateAggregate(){HashSet = new HashSet<string>{"Apples", "Oranges"}};
            var createUnitOfWork =  new CollectionUnitOfWork(Database, CollectionName);
            createUnitOfWork.Collections.Add(createdModel);
            await createUnitOfWork.CommitAsync();

            var updateUnitOfWork = new CollectionUnitOfWork(Database, CollectionName);
            var updateModel = await updateUnitOfWork.Collections.QuerySingleAsync(m => m.Id == createdModel.Id);
            updateModel.HashSet.Add("Pears");
            updateModel.HashSet.Remove("Apples");
            await updateUnitOfWork.CommitAsync();

            var updatedModel = await updateUnitOfWork.Collections.QuerySingleAsync(m => m.Id == createdModel.Id);
            CollectionAssert.AreEquivalent(new[]{"Oranges", "Pears"}, updatedModel.HashSet);
        }

        private async Task<CustomerAggregate> CreateExistingCustomerAggregate()
        {
            var createUnitOfWork = new CustomerUnitOfWork(Database, CollectionName);
            var createdCustomer = new CustomerAggregate
            {
                Name = "John Smith",
                DefaultAddresses = new CustomerDefaultAddresses
                {
                    InvoiceAddress = new AddressAggregate
                    {
                        HouseName = "Invoice Address"
                    }
                }
            };
            createUnitOfWork.Customers.Add(createdCustomer);
            await createUnitOfWork.CommitAsync();
            return createdCustomer;
        }
    }
}
