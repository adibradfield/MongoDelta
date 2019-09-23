using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using NUnit.Framework;

namespace MongoDelta.IntegrationTests
{
    public class RemoveRecordTests
    {
        private string _connectionString;
        private string _databaseName;
        private MongoClient _client;
        private IMongoDatabase _database;

        [OneTimeSetUp]
        public void Setup()
        {
            _connectionString = "mongodb://localhost:27017";
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
        public async Task AddAndRemove_SimpleObject_Success()
        {
            var collectionName = Guid.NewGuid().ToString();
            var collection = _database.GetCollection<UserAggregate>(collectionName);
            var testUser = await AddTestUser(collection);

            var removeRepository = new MongoDeltaRepository<UserAggregate>(collection);
            var model = await removeRepository.QuerySingleAsync(query =>
                query.Where(user => user.Id == testUser.Id));
            removeRepository.Remove(model);
            await removeRepository.CommitChangesAsync();

            var removeQueryResult = await removeRepository.QuerySingleAsync(query => query.Where(user => user.FirstName == "John"));
            Assert.IsNull(removeQueryResult);
        }

        private static async Task<UserAggregate> AddTestUser(IMongoCollection<UserAggregate> collection)
        {
            var addRepository = new MongoDeltaRepository<UserAggregate>(collection);
            addRepository.Add(new UserAggregate()
            {
                FirstName = "John",
                Surname = "Smith"
            });
            await addRepository.CommitChangesAsync();

            var queryResult = await addRepository.QuerySingleAsync(query => query.Where(user => user.FirstName == "John"));
            Assert.IsNotNull(queryResult);
            return queryResult;
        }
    }
}