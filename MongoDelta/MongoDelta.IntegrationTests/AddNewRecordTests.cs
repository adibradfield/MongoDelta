using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using NUnit.Framework;

namespace MongoDelta.IntegrationTests
{
    public class AddNewRecordTests
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
        public async Task AddAndRetrieve_SimpleObject_Success()
        {
            var collectionName = Guid.NewGuid().ToString();
            var collection = _database.GetCollection<UserAggregate>(collectionName);
            var repository = new MongoDeltaRepository<UserAggregate>(collection);
            repository.Add(new UserAggregate()
            {
                FirstName = "John",
                Surname = "Smith"
            });
            await repository.CommitChangesAsync();

            var queryResult = await repository.QuerySingleAsync(query => query.Where(user => user.FirstName == "John"));
            Assert.IsNotNull(queryResult);
        }

        [Test]
        public async Task AddAndRetrieve_MultipleSimpleObjects_Success()
        {
            var collectionName = Guid.NewGuid().ToString();
            var collection = _database.GetCollection<UserAggregate>(collectionName);
            var repository = new MongoDeltaRepository<UserAggregate>(collection);

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

            repository.Add(person1);
            repository.Add(person2);
            await repository.CommitChangesAsync();

            var person1Result = await repository.QuerySingleAsync(query => query.Where(user => user.Id == person1.Id));
            var person2Result = await repository.QuerySingleAsync(query => query.Where(user => user.Id == person2.Id));
            Assert.IsNotNull(person1Result);
            Assert.IsNotNull(person2Result);
        }
    }

    class UserAggregate
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string Surname { get; set; }
    }
}