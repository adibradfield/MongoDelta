using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDelta.IntegrationTests.Models;
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
        public async Task AddAndRetrieve_SimpleObject_Success()
        {
            var unitOfWork = new UserUnitOfWork(_database);
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
            var unitOfWork = new UserUnitOfWork(_database);

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