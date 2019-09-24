using System;
using MongoDB.Driver;
using MongoDelta.IntegrationTests.Helpers;
using NUnit.Framework;

namespace MongoDelta.IntegrationTests
{
    public abstract class MongoTestBase
    {
        private MongoClient _client;
        protected IMongoDatabase Database;
        protected string CollectionName;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _client = new MongoClient(MongoConnectionHelper.GetConnectionString());
            Database = _client.GetDatabase("test");
        }

        [SetUp]
        public void Setup()
        {
            CollectionName = Guid.NewGuid().ToString();
        }

        [TearDown]
        public void TearDown()
        {
            Database.DropCollection(CollectionName);
        }
    }
}
