using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Moq;
using NUnit.Framework;

namespace MongoDelta.UnitTests.MongoDeltaRepository
{
    [TestFixture]
    public class PersistChanges
    {
        [Test]
        public async Task AddSingle_Success()
        {
            var model = new TestAggregate();

            var collection = SetupCollectionForInsert(model);

            var repository = new MongoDeltaRepository<TestAggregate>(collection.Object);
            repository.Add(model);
            await repository.PersistChangesAsync();
            collection.Verify();
        }

        [Test]
        public async Task AddNone_DoesNotCallDatabase_Success()
        {
            var collection = SetupCollectionForNoInsert();

            var repository = new MongoDeltaRepository<TestAggregate>(collection.Object);
            await repository.PersistChangesAsync();
            collection.Verify(c => c.InsertManyAsync(It.IsAny<IClientSessionHandle>(), It.IsAny<IEnumerable<TestAggregate>>(),
                It.IsAny<InsertManyOptions>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        private static Mock<IMongoCollection<TestAggregate>> SetupCollectionForInsert(TestAggregate model)
        {
            var collection = SetupCollectionWithSession(out var session);

            var insertManyMethod = collection.Setup(c => c.InsertManyAsync(session, It.IsAny<IEnumerable<TestAggregate>>(),
                It.IsAny<InsertManyOptions>(), It.IsAny<CancellationToken>()));
            insertManyMethod.Verifiable();
            insertManyMethod.Callback((IClientSessionHandle sessionHandle, IEnumerable<TestAggregate> documents,
                InsertManyOptions options, CancellationToken ctx) =>
            {
                var insertedModel = documents.Single();
                Assert.AreEqual(model, insertedModel);
            });
            return collection;
        }

        private static Mock<IMongoCollection<TestAggregate>> SetupCollectionForNoInsert()
        {
            var collection = SetupCollectionWithSession(out _);
            return collection;
        }

        private static Mock<IMongoCollection<TestAggregate>> SetupCollectionWithSession(out IClientSessionHandle session)
        {
            var collection = new Mock<IMongoCollection<TestAggregate>>();
            var database = new Mock<IMongoDatabase>();
            var client = new Mock<IMongoClient>();
            session = new Mock<IClientSessionHandle>().Object;

            collection.SetupGet(c => c.Database).Returns(database.Object);
            database.SetupGet(d => d.Client).Returns(client.Object);
            client.Setup(c => c.StartSession(It.IsAny<ClientSessionOptions>(), It.IsAny<CancellationToken>()))
                .Returns(session);
            return collection;
        }

        public class TestAggregate{}
    }
}