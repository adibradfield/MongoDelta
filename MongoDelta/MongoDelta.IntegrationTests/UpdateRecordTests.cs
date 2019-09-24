using System.Threading.Tasks;
using MongoDB.Driver.Linq;
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
            var model = await unitOfWork.Users.QuerySingleAsync(query =>
                query.Where(user => user.Id == testUser.Id));

            model.FirstName = "Bobby";
            await unitOfWork.CommitAsync();

            var updateQuery = await unitOfWork.Users.QuerySingleAsync(query => query.Where(user => user.FirstName == "Bobby"));
            Assert.IsNotNull(updateQuery);
        }
    }
}
