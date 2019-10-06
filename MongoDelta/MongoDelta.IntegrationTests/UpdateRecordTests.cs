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
    }
}
