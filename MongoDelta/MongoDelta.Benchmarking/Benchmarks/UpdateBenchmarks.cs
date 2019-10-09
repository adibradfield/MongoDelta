using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDelta.Benchmarking.Models;

namespace MongoDelta.Benchmarking.Benchmarks
{
    public class UpdateBenchmarks
    {
        [Params(1, 50, 500)]
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
            var records = new List<DeltaUserModel>();
            for (var i = 0; i < 500; i++)
            {
                records.Add(new DeltaUserModel());
            }
            _database.GetCollection<DeltaUserModel>(_collectionName).InsertMany(records);
        }

        #endregion

        [Benchmark(Description = "Mongo DB Driver - Bulk Update", Baseline = true)]
        public async Task MongoDbDriver_BulkUpdate()
        {
            var collection = _database.GetCollection<DeltaUserModel>(_collectionName);
            var records = await collection.AsQueryable().Take(NumberOfRecords).ToListAsync();

            var writeModels = new List<WriteModel<DeltaUserModel>>();
            foreach (var record in records)
            {
                var filter = new FilterDefinitionBuilder<DeltaUserModel>().Eq(user => user.Id, record.Id);
                var updateDefinition = new UpdateDefinitionBuilder<DeltaUserModel>()
                    .Set(user => user.DisplayName, Guid.NewGuid().ToString())
                    .Set(user => user.EmailAddress.EmailAddress, Guid.NewGuid().ToString())
                    .Set(user => user.EmailAddress.Verified, record.EmailAddress.Verified);
                writeModels.Add(new UpdateOneModel<DeltaUserModel>(filter, updateDefinition));
            }

            await collection.BulkWriteAsync(writeModels);
        }

        [Benchmark(Description = "MongoDelta - Delta Update Strategy")]
        public async Task MongoDelta_ReplaceStrategy()
        {
            var unitOfWork = new UserUnitOfWork(_database, _collectionName);

            var records = await unitOfWork.DeltaUsers.QueryAsync(query => query.Take(NumberOfRecords));
            foreach (var record in records)
            {
                record.DisplayName = Guid.NewGuid().ToString();
                record.EmailAddress.EmailAddress = Guid.NewGuid().ToString();
                record.EmailAddress.Verified = !record.EmailAddress.Verified;
            }

            await unitOfWork.CommitAsync();
        }
    }
}
