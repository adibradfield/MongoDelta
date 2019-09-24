using System.Threading.Tasks;
using MongoDB.Driver.Linq;
using MongoDelta.IntegrationTests.Models;
using NUnit.Framework;

namespace MongoDelta.IntegrationTests
{
    [TestFixture]
    public class AddNewRecordTests : MongoTestBase
    {
        [Test]
        public async Task AddAndRetrieve_SimpleObject_Success()
        {
            var unitOfWork = new UserUnitOfWork(Database, CollectionName);
            unitOfWork.Users.Add(new UserAggregate()
            {
                FirstName = "John",
                Surname = "Smith"
            });
            await unitOfWork.CommitAsync();

            var queryResult = await unitOfWork.Users.QuerySingleAsync(query => query.Where(user => user.FirstName == "John"));
            Assert.IsNotNull(queryResult);
        }

        [Test]
        public async Task AddAndRetrieve_MultipleSimpleObjects_Success()
        {
            var unitOfWork = new UserUnitOfWork(Database, CollectionName);

            var person1 = new UserAggregate()
            {
                FirstName = "John",
                Surname = "Smith"
            };
            var person2 = new UserAggregate()
            {
                FirstName = "Jane",
                Surname = "Doe"
            };

            unitOfWork.Users.Add(person1);
            unitOfWork.Users.Add(person2);
            await unitOfWork.CommitAsync();

            var person1Result = await unitOfWork.Users.QuerySingleAsync(query => query.Where(user => user.Id == person1.Id));
            var person2Result = await unitOfWork.Users.QuerySingleAsync(query => query.Where(user => user.Id == person2.Id));
            Assert.IsNotNull(person1Result);
            Assert.IsNotNull(person2Result);
        }
    }
}