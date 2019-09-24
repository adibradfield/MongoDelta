﻿using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDelta.IntegrationTests.Models;
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
        public async Task AddAndRemove_SimpleObject_Success()
        {
            var collectionName = Guid.NewGuid().ToString();
            var testUser = await UserAggregate.AddTestUser(_database, collectionName);

            var unitOfWork = new UserUnitOfWork(_database, collectionName);
            var model = await unitOfWork.Users.QuerySingleAsync(query =>
                query.Where(user => user.Id == testUser.Id));
            unitOfWork.Users.Remove(model);
            await unitOfWork.CommitAsync();

            var removeQueryResult = await unitOfWork.Users.QuerySingleAsync(query => query.Where(user => user.FirstName == "John"));
            Assert.IsNull(removeQueryResult);
        }
    }
}