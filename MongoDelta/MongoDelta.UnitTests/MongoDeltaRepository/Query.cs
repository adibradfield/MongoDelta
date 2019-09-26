using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDelta.MongoDbHelpers;
using MongoDelta.UnitTests.Models;
using Moq;
using NUnit.Framework;

namespace MongoDelta.UnitTests.MongoDeltaRepository
{
    [TestFixture]
    public class Query
    {
        [Test]
        public async Task Query_MultipleResults_Success()
        {
            var results = new List<BlankAggregate>()
            {
                new BlankAggregate(),
                new BlankAggregate()
            };

            var repository = CreateRepositoryForResults(results);

            var queryResults = await repository.QueryAsync(query => query);
            Assert.AreEqual(results.Count(), queryResults.Count);
        }

        [Test]
        public async Task QuerySingle_SingleResult_Success()
        {
            var results = new List<BlankAggregate>()
            {
                new BlankAggregate()
            };

            var repository = CreateRepositoryForResults(results);

            BlankAggregate queryResult = await repository.QuerySingleAsync(query => query);
            Assert.IsNotNull(queryResult);
        }

        [Test]
        public async Task QuerySingle_NoResults_Success()
        {
            var results = new List<BlankAggregate>();

            var repository = CreateRepositoryForResults(results);

            BlankAggregate queryResult = await repository.QuerySingleAsync(query => query);
            Assert.IsNull(queryResult);
        }

        [Test]
        public void QuerySingle_MultipleResults_Failure()
        {
            var results = new List<BlankAggregate>()
            {
                new BlankAggregate(),
                new BlankAggregate()
            };

            var repository = CreateRepositoryForResults(results);

            Assert.ThrowsAsync<InvalidOperationException>(async () => { await repository.QuerySingleAsync(query => query); });
        }

        private static MongoDeltaRepository<BlankAggregate> CreateRepositoryForResults(List<BlankAggregate> results)
        {
            var collection = new Mock<IMongoCollection<BlankAggregate>>(MockBehavior.Strict);
            var collectionToQueryableConverter = new Mock<IMongoCollectionToQueryableConverter>(MockBehavior.Strict);
            var queryable = new Mock<IMongoQueryable<BlankAggregate>>();
            collectionToQueryableConverter.Setup(c => c.GetQueryable(collection.Object)).Returns(queryable.Object);
            var queryRunner = new Mock<IMongoQueryRunner>();
            queryRunner.Setup(q => q.RunAsync(It.IsAny<IMongoQueryable<BlankAggregate>>()))
                .Returns(Task.FromResult<IReadOnlyCollection<BlankAggregate>>(results));
            queryRunner.Setup(q => q.RunSingleAsync(It.IsAny<IMongoQueryable<BlankAggregate>>()))
                .Returns(() => Task.FromResult(results.SingleOrDefault()));

            var repository = new MongoDeltaRepository<BlankAggregate>(collection.Object, collectionToQueryableConverter.Object, queryRunner.Object);
            return repository;
        }
    }
}