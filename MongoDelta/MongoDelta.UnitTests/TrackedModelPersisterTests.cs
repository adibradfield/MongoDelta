using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDelta.ChangeTracking;
using MongoDelta.UnitTests.Models;
using NUnit.Framework;

namespace MongoDelta.UnitTests
{
    [TestFixture]
    public class TrackedModelPersisterTests
    {
        [Test]
        public void Persist_SingleAdded_Success()
        {
            var trackedCollection = new TrackedModelCollection<BlankAggregate>();
            var model = new BlankAggregate();
            trackedCollection.New(model);

            var persister = new TrackedModelPersister<BlankAggregate>();
            var changes = persister.GetChangesForWrite(trackedCollection).ToArray();
            Assert.AreEqual(1, changes.Count());
            Assert.IsTrue(changes.All(c => c.ModelType == WriteModelType.InsertOne), "Expected all changes to be inserts");
        }

        [Test]
        public void Persist_TwoAdded_Success()
        {
            var trackedCollection = new TrackedModelCollection<BlankAggregate>();
            var model1 = new BlankAggregate();
            var model2 = new BlankAggregate();
            trackedCollection.New(model1);
            trackedCollection.New(model2);

            var persister = new TrackedModelPersister<BlankAggregate>();
            var changes = persister.GetChangesForWrite(trackedCollection).ToArray();
            Assert.AreEqual(2, changes.Count());
            Assert.IsTrue(changes.All(c => c.ModelType == WriteModelType.InsertOne), "Expected all changes to be inserts");
        }

        [Test]
        public void Persist_NoneAdded_DatabaseNotCalled()
        {
            var trackedCollection = new TrackedModelCollection<BlankAggregate>();

            var persister = new TrackedModelPersister<BlankAggregate>();
            var changes = persister.GetChangesForWrite(trackedCollection).ToArray();

            Assert.AreEqual(0, changes.Count());
        }

        [Test]
        public void Persist_SingleRemoved_Success()
        {
            var trackedCollection = new TrackedModelCollection<GuidAggregate>();
            var model1 = new GuidAggregate();
            var model2 = new GuidAggregate();
            trackedCollection.Existing(model1);
            trackedCollection.Existing(model2);
            trackedCollection.Remove(model1);

            var persister = new TrackedModelPersister<GuidAggregate>();
            var changes = persister.GetChangesForWrite(trackedCollection).ToArray();
            Assert.AreEqual(1, changes.Count());

            AssertModelsDeleted(changes, model1.Id);
        }

        [Test]
        public void Persist_TwoRemoved_Success()
        {
            var trackedCollection = new TrackedModelCollection<GuidAggregate>();
            var model1 = new GuidAggregate();
            var model2 = new GuidAggregate();
            trackedCollection.Existing(model1);
            trackedCollection.Existing(model2);
            trackedCollection.Remove(model1);
            trackedCollection.Remove(model2);

            var persister = new TrackedModelPersister<GuidAggregate>();
            var changes = persister.GetChangesForWrite(trackedCollection).ToArray();
            Assert.AreEqual(2, changes.Count());

            AssertModelsDeleted(changes, model1.Id, model2.Id);
        }

        [Test]
        public void Persist_NoneRemoved_DatabaseNotCalled()
        {
            var trackedCollection = new TrackedModelCollection<GuidAggregate>();
            var model1 = new GuidAggregate();
            var model2 = new GuidAggregate();
            trackedCollection.Existing(model1);
            trackedCollection.Existing(model2);

            var persister = new TrackedModelPersister<GuidAggregate>();
            var changes = persister.GetChangesForWrite(trackedCollection).ToArray();

            Assert.AreEqual(0, changes.Count());
        }

        [Test]
        public void Persist_SingleUpdated_Success()
        {
            var trackedCollection = new TrackedModelCollection<FlatAggregate>();
            var model = new FlatAggregate();
            trackedCollection.Existing(model);
            model.Name = "Jane Doe";

            var persister = new TrackedModelPersister<FlatAggregate>();
            var changes = persister.GetChangesForWrite(trackedCollection).ToArray();

            Assert.AreEqual(1, changes.Count());
            AssertModelsUpdated(changes, model.Id);
        }

        [Test]
        public void Persist_TwoUpdated_Success()
        {
            var trackedCollection = new TrackedModelCollection<FlatAggregate>();
            var model1 = new FlatAggregate();
            var model2 = new FlatAggregate();
            trackedCollection.Existing(model1);
            trackedCollection.Existing(model2);
            model1.Name = "Jane Doe";
            model2.Name = "Bob Sinclair";

            var persister = new TrackedModelPersister<FlatAggregate>();
            var changes = persister.GetChangesForWrite(trackedCollection).ToArray();

            Assert.AreEqual(2, changes.Count());
            AssertModelsUpdated(changes, model1.Id, model2.Id);
        }

        [Test]
        public void Persist_NoneUpdated_DatabaseNotCalled()
        {
            var trackedCollection = new TrackedModelCollection<FlatAggregate>();
            var model1 = new FlatAggregate();
            var model2 = new FlatAggregate();
            trackedCollection.Existing(model1);
            trackedCollection.Existing(model2);

            var persister = new TrackedModelPersister<FlatAggregate>();
            var changes = persister.GetChangesForWrite(trackedCollection).ToArray();

            Assert.AreEqual(0, changes.Count());
        }

        private static void AssertModelsDeleted<T>(IEnumerable<WriteModel<T>> changes, params Guid[] expectedDeletedIds)
        {
            var mapper = BsonClassMap.LookupClassMap(typeof(T));
            var deletedGuids = changes.Cast<DeleteOneModel<T>>().Select(c => c.Filter
                .Render(new BsonClassMapSerializer<T>(mapper), new BsonSerializerRegistry())["_id"].AsGuid);
            
            CollectionAssert.AreEquivalent(expectedDeletedIds, deletedGuids);
        }

        private static void AssertModelsUpdated<T>(IEnumerable<WriteModel<T>> changes, params Guid[] expectedDeletedIds)
        {
            var mapper = BsonClassMap.LookupClassMap(typeof(T));
            var deletedGuids = changes.Cast<ReplaceOneModel<T>>().Select(c => c.Filter
                .Render(new BsonClassMapSerializer<T>(mapper), new BsonSerializerRegistry())["_id"].AsGuid);
            
            CollectionAssert.AreEquivalent(expectedDeletedIds, deletedGuids);
        }
    }
}