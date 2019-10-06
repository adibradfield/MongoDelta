using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDelta.ChangeTracking;
using MongoDelta.UnitTests.Models;
using MongoDelta.UpdateStrategies;
using Moq;
using NUnit.Framework;

namespace MongoDelta.UnitTests.UpdateStrategies
{
    [TestFixture]
    public class DeltaUpdateStrategyTests
    {
        [Test]
        public async Task Update_SingleChange_Success()
        {
            var expectedUpdateDefinition = new BsonDocument("$set", new BsonDocument("Name", "John Smith"));
            var session = SetupMongoCollectionForUpdate(expectedUpdateDefinition, out var collection);
            var model = GetTrackedFlatAggregate(out var trackedModel);

            model.Name = "John Smith";

            var strategy = new DeltaUpdateStrategy<FlatAggregate>();
            await strategy.Update(session.Object, collection.Object, trackedModel);
        }

        [Test]
        public async Task Update_TwoChanges_Success()
        {
            var expectedUpdateDefinition = new BsonDocument("$set", new BsonDocument
            {
                new BsonElement("Age", 25),
                new BsonElement("Name", "John Smith"),
            });
            var session = SetupMongoCollectionForUpdate(expectedUpdateDefinition, out var collection);
            var model = GetTrackedFlatAggregate(out var trackedModel);

            model.Name = "John Smith";
            model.Age = 25;

            var strategy = new DeltaUpdateStrategy<FlatAggregate>();
            await strategy.Update(session.Object, collection.Object, trackedModel);
        }

        private static FlatAggregate GetTrackedFlatAggregate(out TrackedModel<FlatAggregate> trackedModel)
        {
            var model = new FlatAggregate()
            {
                Id = Guid.NewGuid(),
                Name = "Bob Smith",
                Age = 20,
                DateOfBirth = new DateTime(2000, 01, 01)
            };
            var trackingCollection = new TrackedModelCollection<FlatAggregate>();
            trackingCollection.Existing(model);
            trackedModel = trackingCollection.Single();
            return model;
        }

        private static Mock<IClientSessionHandle> SetupMongoCollectionForUpdate(BsonDocument expectedUpdateDefinition, out Mock<IMongoCollection<FlatAggregate>> collection)
        {
            var session = new Mock<IClientSessionHandle>(MockBehavior.Strict);

            collection = new Mock<IMongoCollection<FlatAggregate>>(MockBehavior.Strict);
            var updateMethod = collection.Setup(m => m.UpdateOneAsync(session.Object,
                It.IsAny<FilterDefinition<FlatAggregate>>(), It.IsAny<UpdateDefinition<FlatAggregate>>(),
                It.IsAny<UpdateOptions>(), It.IsAny<CancellationToken>()));

            updateMethod.Callback((IClientSessionHandle clientSession, FilterDefinition<FlatAggregate> filter,
                UpdateDefinition<FlatAggregate> updateDefinition, UpdateOptions options,
                CancellationToken cancellationToken) =>
            {
                var actualUpdateDefinition = updateDefinition.ToBsonDocument()["Document"];
                CollectionAssert.AreEquivalent(expectedUpdateDefinition["$set"].ToBsonDocument(), actualUpdateDefinition["$set"].ToBsonDocument());
            });

            updateMethod.Returns(Task.FromResult<UpdateResult>(new UpdateResult.Acknowledged(1, 1, null)));
            return session;
        }
    }
}
