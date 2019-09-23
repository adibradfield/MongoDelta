using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDelta.UnitTests.Helpers;
using MongoDelta.UnitTests.Models;
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
            var model = new BlankAggregate();

            var collection = SetupCollectionForInsert(model);

            var repository = new MongoDeltaRepository<BlankAggregate>(collection.Object);
            repository.Add(model);
            await repository.CommitChangesAsync();
            collection.Verify();
        }

        [Test]
        public async Task AddNone_DoesNotCallDatabase_Success()
        {
            var collection = SetupCollectionForNoInsert();

            var repository = new MongoDeltaRepository<BlankAggregate>(collection.Object);
            await repository.CommitChangesAsync();
            collection.Verify(c => c.InsertManyAsync(It.IsAny<IClientSessionHandle>(), It.IsAny<IEnumerable<BlankAggregate>>(),
                It.IsAny<InsertManyOptions>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        private static Mock<IMongoCollection<BlankAggregate>> SetupCollectionForInsert(BlankAggregate model)
        {
            var collection = MongoCollectionHelper.SetupCollectionWithSession<BlankAggregate>(out var session);

            var insertManyMethod = collection.Setup(c => c.InsertManyAsync(session, It.IsAny<IEnumerable<BlankAggregate>>(),
                It.IsAny<InsertManyOptions>(), It.IsAny<CancellationToken>()));
            insertManyMethod.Verifiable();
            insertManyMethod.Callback((IClientSessionHandle sessionHandle, IEnumerable<BlankAggregate> documents,
                InsertManyOptions options, CancellationToken ctx) =>
            {
                var insertedModel = documents.Single();
                Assert.AreEqual(model, insertedModel);
            });
            return collection;
        }

        private static Mock<IMongoCollection<BlankAggregate>> SetupCollectionForNoInsert()
        {
            var collection = MongoCollectionHelper.SetupCollectionWithSession<BlankAggregate>(out _);
            return collection;
        }
    }
}