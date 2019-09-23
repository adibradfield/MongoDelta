using System.Linq;
using System.Threading.Tasks;
using MongoDelta.UnitTests.Helpers;
using MongoDelta.UnitTests.Models;
using NUnit.Framework;

namespace MongoDelta.UnitTests
{
    [TestFixture]
    public class TrackedModelPersisterTests
    {
        [Test]
        public async Task Persist_SingleAdded_Success()
        {
            var collection = MongoCollectionHelper.SetupCollectionWithSession<BlankAggregate>(out var session);
            var trackedCollection = new TrackedModelCollection<BlankAggregate>();
            var model = new BlankAggregate();
            trackedCollection.New(model);

            collection.ExpectObjectsInserted(documents =>
            {
                Assert.AreEqual(1, documents.Count());
                Assert.AreEqual(model, documents.Single());
            });

            var persister = new TrackedModelPersister<BlankAggregate>();
            await persister.PersistChangesAsync(collection.Object, trackedCollection, session);
            collection.Verify();
            collection.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Persist_TwoAdded_Success()
        {
            var collection = MongoCollectionHelper.SetupCollectionWithSession<BlankAggregate>(out var session);
            var trackedCollection = new TrackedModelCollection<BlankAggregate>();
            var model1 = new BlankAggregate();
            var model2 = new BlankAggregate();
            trackedCollection.New(model1);
            trackedCollection.New(model2);

            collection.ExpectObjectsInserted(documents =>
            {
                Assert.AreEqual(2, documents.Count());
                CollectionAssert.Contains(documents, model1);
                CollectionAssert.Contains(documents, model2);
            });

            var persister = new TrackedModelPersister<BlankAggregate>();
            await persister.PersistChangesAsync(collection.Object, trackedCollection, session);
            collection.Verify();
            collection.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Persist_NoneAdded_DatabaseNotCalled()
        {
            var collection = MongoCollectionHelper.SetupCollectionWithSession<BlankAggregate>(out var session);
            var trackedCollection = new TrackedModelCollection<BlankAggregate>();

            var persister = new TrackedModelPersister<BlankAggregate>();
            await persister.PersistChangesAsync(collection.Object, trackedCollection, session);

            collection.VerifyNoObjectsInserted();
            collection.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Persist_SingleRemoved_Success()
        {
            var collection = MongoCollectionHelper.SetupCollectionWithSession<GuidAggregate>(out var session);
            var trackedCollection = new TrackedModelCollection<GuidAggregate>();
            var model1 = new GuidAggregate();
            var model2 = new GuidAggregate();
            trackedCollection.Existing(model1);
            trackedCollection.Existing(model2);
            trackedCollection.Remove(model1);
            collection.ExpectObjectsDeleted(model1.Id);

            var persister = new TrackedModelPersister<GuidAggregate>();
            await persister.PersistChangesAsync(collection.Object, trackedCollection, session);

            collection.Verify();
            collection.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Persist_TwoRemoved_Success()
        {
            var collection = MongoCollectionHelper.SetupCollectionWithSession<GuidAggregate>(out var session);
            var trackedCollection = new TrackedModelCollection<GuidAggregate>();
            var model1 = new GuidAggregate();
            var model2 = new GuidAggregate();
            trackedCollection.Existing(model1);
            trackedCollection.Existing(model2);
            trackedCollection.Remove(model1);
            trackedCollection.Remove(model2);
            collection.ExpectObjectsDeleted(model1.Id, model2.Id);

            var persister = new TrackedModelPersister<GuidAggregate>();
            await persister.PersistChangesAsync(collection.Object, trackedCollection, session);

            collection.Verify();
            collection.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Persist_NoneRemoved_DatabaseNotCalled()
        {
            var collection = MongoCollectionHelper.SetupCollectionWithSession<GuidAggregate>(out var session);
            var trackedCollection = new TrackedModelCollection<GuidAggregate>();
            var model1 = new GuidAggregate();
            var model2 = new GuidAggregate();
            trackedCollection.Existing(model1);
            trackedCollection.Existing(model2);

            var persister = new TrackedModelPersister<GuidAggregate>();
            await persister.PersistChangesAsync(collection.Object, trackedCollection, session);

            collection.VerifyNoObjectsDeleted();
            collection.VerifyNoOtherCalls();
        }
    }
}