using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Moq;
using NUnit.Framework;

namespace MongoDelta.UnitTests.Helpers
{
    public static class MongoCollectionHelper
    {
        public static Mock<IMongoCollection<TAggregate>> SetupCollectionWithSession<TAggregate>(out IClientSessionHandle session)
        {
            var collection = new Mock<IMongoCollection<TAggregate>>();
            var database = new Mock<IMongoDatabase>();
            var client = new Mock<IMongoClient>();
            session = new Mock<IClientSessionHandle>().Object;

            collection.SetupGet(c => c.Database).Returns(database.Object);
            database.SetupGet(d => d.Client).Returns(client.Object);
            client.Setup(c => c.StartSession(It.IsAny<ClientSessionOptions>(), It.IsAny<CancellationToken>()))
                .Returns(session);
            return collection;
        }

        public static void ExpectObjectsInserted<TAggregate>(this Mock<IMongoCollection<TAggregate>> collection,
            Action<List<TAggregate>> assertOnInsert)
        {
            var insertMethod = collection.Setup(c => c.InsertManyAsync(It.IsAny<IClientSessionHandle>(),
                It.IsAny<IEnumerable<TAggregate>>(), It.IsAny<InsertManyOptions>(), It.IsAny<CancellationToken>()));
            insertMethod.Verifiable();
            insertMethod.Callback((IClientSessionHandle session, IEnumerable<TAggregate> documents,
                InsertManyOptions options, CancellationToken cancellationToken) =>
            {
                assertOnInsert(documents.ToList());
            });
            insertMethod.Returns(Task.CompletedTask);
        }

        public static void VerifyNoObjectsInserted<TAggregate>(this Mock<IMongoCollection<TAggregate>> collection)
        {
            collection.Verify(c => c.InsertManyAsync(It.IsAny<IClientSessionHandle>(),
                It.IsAny<IEnumerable<TAggregate>>(), It.IsAny<InsertManyOptions>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        public static void ExpectObjectsDeleted<TAggregate>(
            this Mock<IMongoCollection<TAggregate>> collection, params Guid[] idsToDelete)
        {
            var deleteMethod = collection.Setup(c => c.DeleteManyAsync(It.IsAny<IClientSessionHandle>(), It.IsAny<FilterDefinition<TAggregate>>(), It.IsAny<DeleteOptions>(), It.IsAny<CancellationToken>()));
            deleteMethod.Verifiable();
            deleteMethod.Callback((IClientSessionHandle session, FilterDefinition<TAggregate> filter, DeleteOptions options,
                CancellationToken cancellationToken) =>
            {
                var mapper = BsonClassMap.LookupClassMap(typeof(TAggregate));
                var document = filter.Render(new BsonClassMapSerializer<TAggregate>(mapper), new BsonSerializerRegistry());

                var filterExpression = document["_id"];
                var ids = filterExpression.AsBsonDocument[0].AsBsonArray.ToArray().Select(b => b.AsBsonBinaryData.AsGuid);
                CollectionAssert.AreEquivalent(idsToDelete, ids);
            });
            deleteMethod.Returns(Task.FromResult<DeleteResult>(new DeleteResult.Acknowledged(idsToDelete.Length)));
        }

        public static void VerifyNoObjectsDeleted<TAggregate>(this Mock<IMongoCollection<TAggregate>> collection)
        {
            collection.Verify(c => c.DeleteManyAsync(It.IsAny<IClientSessionHandle>(),
                It.IsAny<FilterDefinition<TAggregate>>(), It.IsAny<DeleteOptions>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
