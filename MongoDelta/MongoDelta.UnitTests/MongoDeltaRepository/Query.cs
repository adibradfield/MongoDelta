using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
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
            var results = new List<TestAggregate>()
            {
                new TestAggregate(),
                new TestAggregate()
            };

            var repository = CreateRepositoryForResults(results);

            var queryResults = await repository.QueryAsync(query => query);
            Assert.AreEqual(results.Count(), queryResults.Count);
        }

        [Test]
        public async Task QuerySingle_SingleResult_Success()
        {
            var results = new List<TestAggregate>()
            {
                new TestAggregate()
            };

            var repository = CreateRepositoryForResults(results);

            TestAggregate queryResult = await repository.QuerySingleAsync(query => query);
            Assert.IsNotNull(queryResult);
        }

        [Test]
        public async Task QuerySingle_NoResults_Success()
        {
            var results = new List<TestAggregate>();

            var repository = CreateRepositoryForResults(results);

            TestAggregate queryResult = await repository.QuerySingleAsync(query => query);
            Assert.IsNull(queryResult);
        }

        [Test]
        public void QuerySingle_MultipleResults_Failure()
        {
            var results = new List<TestAggregate>()
            {
                new TestAggregate(),
                new TestAggregate()
            };

            var repository = CreateRepositoryForResults(results);

            Assert.ThrowsAsync<InvalidOperationException>(async () => { await repository.QuerySingleAsync(query => query); });
        }

        private static MongoDeltaRepository<TestAggregate> CreateRepositoryForResults(List<TestAggregate> results)
        {
            var collection = new Mock<IMongoCollection<TestAggregate>>(MockBehavior.Strict);
            var collectionToQueryableConverter = new Mock<IMongoCollectionToQueryableConverter>(MockBehavior.Strict);
            var queryable = new Mock<IMongoQueryable<TestAggregate>>();
            collectionToQueryableConverter.Setup(c => c.GetQueryable(collection.Object)).Returns(queryable.Object);
            var queryRunner = new Mock<IMongoQueryRunner>();
            queryRunner.Setup(q => q.RunAsync(It.IsAny<IMongoQueryable<TestAggregate>>()))
                .Returns(Task.FromResult<IReadOnlyCollection<TestAggregate>>(results));

            var repository = new MongoDeltaRepository<TestAggregate>(collection.Object, collectionToQueryableConverter.Object, queryRunner.Object);
            return repository;
        }

        public class TestAggregate{}
    }
}