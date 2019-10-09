using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDelta.Benchmarking.Models;

namespace MongoDelta.Benchmarking.Benchmarks
{
    public class ReplaceBenchmarks
    {
        [Params(1, 10, 50, 150, 500)]
        public int NumberOfRecords { get; set; }

        private IMongoClient _client;
        private IMongoDatabase _database;
        private string _databaseName;
        private string _collectionName;

        #region Setup

        [GlobalSetup]
        public void GlobalSetup()
        {
            _client = new MongoClient("mongodb://localhost:27017/?retryWrites=false");
            _databaseName = Guid.NewGuid().ToString();
            _database = _client.GetDatabase(_databaseName);
            _collectionName = Guid.NewGuid().ToString();
            Insert500Records();
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            _client.DropDatabase(_databaseName);
        }

        private void Insert500Records()
        {
            var records = new List<UserModel>();
            for (var i = 0; i < 500; i++)
            {
                records.Add(new UserModel());
            }
            _database.GetCollection<UserModel>(_collectionName).InsertMany(records);
        }

        #endregion

        [Benchmark(Description = "Mongo DB Driver - ReplaceOne")]
        public async Task MongoDbDriver_ReplaceOne()
        {
            var collection = _database.GetCollection<UserModel>(_collectionName);
            var records = collection.AsQueryable().Take(NumberOfRecords);
            foreach (var record in records)
            {
                ModifyUserRecord(record);
                var filter = new FilterDefinitionBuilder<UserModel>().Eq(user => user.Id, record.Id);
                await collection.ReplaceOneAsync(filter, record);
            }
        }

        [Benchmark(Description = "Mongo DB Driver - Bulk Replace", Baseline = true)]
        public async Task MongoDbDriver_BulkReplace()
        {
            var collection = _database.GetCollection<UserModel>(_collectionName);
            var records = await collection.AsQueryable().Take(NumberOfRecords).ToListAsync();

            var writeModels = new List<WriteModel<UserModel>>();
            foreach (var record in records)
            {
                ModifyUserRecord(record);
                var filter = new FilterDefinitionBuilder<UserModel>().Eq(user => user.Id, record.Id);
                writeModels.Add(new ReplaceOneModel<UserModel>(filter, record));
            }

            await collection.BulkWriteAsync(writeModels);
        }

        [Benchmark(Description = "MongoDelta - Replace Strategy")]
        public async Task MongoDelta_ReplaceStrategy()
        {
            var unitOfWork = new UserUnitOfWork(_database, _collectionName);

            var records = await unitOfWork.Users.QueryAsync(query => query.Take(NumberOfRecords));
            foreach (var record in records)
            {
                ModifyUserRecord(record);
            }

            await unitOfWork.CommitAsync();
        }

        private void ModifyUserRecord(UserModel model)
        {
            model.DisplayName = Guid.NewGuid().ToString();
            model.EmailAddress.EmailAddress = Guid.NewGuid().ToString();
            model.EmailAddress.Verified = !model.EmailAddress.Verified;
        }
    }
}
