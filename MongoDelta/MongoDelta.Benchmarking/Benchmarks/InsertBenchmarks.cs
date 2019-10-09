using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using MongoDB.Driver;
using MongoDelta.Benchmarking.Models;

namespace MongoDelta.Benchmarking.Benchmarks
{
    public class InsertBenchmarks
    {
        [Params(1, 10, 50, 150, 500)]
        public int NumberOfRecords { get; set; }

        private IMongoClient _client;
        private IMongoDatabase _database;
        private string _databaseName;

        #region Setup

        [GlobalSetup]
        public void GlobalSetup()
        {
            _client = new MongoClient("mongodb://localhost:27017/?retryWrites=false");
            _databaseName = Guid.NewGuid().ToString();
            _database = _client.GetDatabase(_databaseName);
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            _client.DropDatabase(_databaseName);
        }

        #endregion

        [Benchmark(Description = "Mongo DB Driver - InsertOne")]
        public async Task MongoDbDriver_IterativeInsertOne()
        {
            var collectionName = Guid.NewGuid().ToString();
            var collection = _database.GetCollection<UserModel>(collectionName);

            for (var i = 0; i < NumberOfRecords; i++)
            {
                await collection.InsertOneAsync(new UserModel());
            }

            await _database.DropCollectionAsync(collectionName);
        }

        [Benchmark(Description = "Mongo DB Driver - InsertMany", Baseline = true)]
        public async Task MongoDbDriver_IterativeInsertMany()
        {
            var collectionName = Guid.NewGuid().ToString();
            var collection = _database.GetCollection<UserModel>(collectionName);

            var records = new List<UserModel>();
            for (var i = 0; i < NumberOfRecords; i++)
            {
                records.Add(new UserModel());
            }
            await collection.InsertManyAsync(records);

            await _database.DropCollectionAsync(collectionName);
        }

        [Benchmark(Description = "MongoDelta")]
        public async Task MongoDelta()
        {
            var collectionName = Guid.NewGuid().ToString();
            var unitOfWork = new UserUnitOfWork(_database, collectionName);

            for (var i = 0; i < NumberOfRecords; i++)
            {
                unitOfWork.Users.Add(new UserModel());
            }
            await unitOfWork.CommitAsync();

            await _database.DropCollectionAsync(collectionName);
        }
    }
}
