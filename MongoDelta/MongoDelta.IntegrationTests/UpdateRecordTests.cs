using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDelta.IntegrationTests.Models;
using NUnit.Framework;

namespace MongoDelta.IntegrationTests
{
    [TestFixture]
    class UpdateRecordTests
    {
        private string _connectionString;
        private string _databaseName;
        private MongoClient _client;
        private IMongoDatabase _database;

        [OneTimeSetUp]
        public void Setup()
        {
            _connectionString = "mongodb://localhost:27017/?retryWrites=false";
            _databaseName = Guid.NewGuid().ToString();
            _client = new MongoClient(_connectionString);
            _database = _client.GetDatabase(_databaseName);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _client.DropDatabase(_databaseName);
        }

        [Test]
        public async Task AddAndUpdate_SimpleObject_Success()
        {
            var collectionName = Guid.NewGuid().ToString();
            var testUser = await UserAggregate.AddTestUser(_database, collectionName);

            var unitOfWork = new UserUnitOfWork(_database, collectionName);
            var model = await unitOfWork.Users.QuerySingleAsync(query =>
                query.Where(user => user.Id == testUser.Id));

            model.FirstName = "Bobby";
            await unitOfWork.CommitAsync();

            var updateQuery = await unitOfWork.Users.QuerySingleAsync(query => query.Where(user => user.FirstName == "Bobby"));
            Assert.IsNotNull(updateQuery);
        }
    }
}
