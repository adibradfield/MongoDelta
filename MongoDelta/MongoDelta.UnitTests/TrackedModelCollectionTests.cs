using System;
using System.Collections.Generic;
using System.Linq;
using MongoDelta.UnitTests.Models;
using NUnit.Framework;

namespace MongoDelta.UnitTests
{
    [TestFixture]
    public class TrackedModelCollectionTests
    {
        [Test]
        public void Add_Single_Success()
        {
            var collection = new TrackedModelCollection<BlankAggregate>();
            var model = new BlankAggregate();

            Assert.DoesNotThrow(() =>
            {
                collection.New(model);
            });

            Assert.AreEqual(model, collection.OfState(TrackedModelState.New).SingleOrDefault()?.Model);
        }

        [Test]
        public void Add_Multiple_Success()
        {
            var collection = new TrackedModelCollection<BlankAggregate>();
            var model1 = new BlankAggregate();
            var model2 = new BlankAggregate();

            Assert.DoesNotThrow(() =>
            {
                collection.New(model1);
                collection.New(model2);
            });

            var addedModels = collection.OfState(TrackedModelState.New).Select(m => m.Model).ToList();
            Assert.AreEqual(2, addedModels.Count());
            Assert.Contains(model1, addedModels);
            Assert.Contains(model2, addedModels);
        }

        [Test]
        public void Add_Null_Failure()
        {
            var collection = new TrackedModelCollection<BlankAggregate>();

            Assert.Throws<ArgumentNullException>(() =>
            {
                collection.New(null);
            });
        }

        [Test]
        public void Add_SameModelTwice_Failure()
        {
            var collection = new TrackedModelCollection<BlankAggregate>();
            var model = new BlankAggregate();

            Assert.Throws<ArgumentException>(() =>
            {
                collection.New(model);
                collection.New(model);
            });
        }

        [Test]
        public void Add_RemovedModel_Success()
        {
            var collection = new TrackedModelCollection<BlankAggregate>();
            var model = new BlankAggregate();
            collection.Existing(model);
            collection.Remove(model);

            Assert.DoesNotThrow(() =>
            {
                collection.New(model);
            });

            Assert.AreEqual(model, collection.OfState(TrackedModelState.Existing).SingleOrDefault()?.Model);
        }

        [Test]
        public void Existing_Single_Success()
        {
            var collection = new TrackedModelCollection<BlankAggregate>();
            var model = new BlankAggregate();

            Assert.DoesNotThrow(() =>
            {
                collection.Existing(model);
            });

            Assert.AreEqual(model, collection.OfState(TrackedModelState.Existing).SingleOrDefault()?.Model);
        }

        [Test]
        public void Existing_Multiple_Success()
        {
            var collection = new TrackedModelCollection<BlankAggregate>();
            var model1 = new BlankAggregate();
            var model2 = new BlankAggregate();

            Assert.DoesNotThrow(() =>
            {
                collection.Existing(model1);
                collection.Existing(model2);
            });

            var addedModels = collection.OfState(TrackedModelState.Existing).Select(m => m.Model).ToList();
            Assert.AreEqual(2, addedModels.Count());
            Assert.Contains(model1, addedModels);
            Assert.Contains(model2, addedModels);
        }

        [Test]
        public void Existing_Null_Failure()
        {
            var collection = new TrackedModelCollection<BlankAggregate>();

            Assert.Throws<ArgumentNullException>(() =>
            {
                collection.Existing(null);
            });
        }

        [Test]
        public void Existing_SameModelTwice_Failure()
        {
            var collection = new TrackedModelCollection<BlankAggregate>();
            var model = new BlankAggregate();

            Assert.Throws<ArgumentException>(() =>
            {
                collection.Existing(model);
                collection.Existing(model);
            });
        }

        [Test]
        public void Remove_Added_Success()
        {
            var collection = new TrackedModelCollection<BlankAggregate>();
            var model = new BlankAggregate();
            collection.New(model);

            Assert.DoesNotThrow(() =>
            {
                collection.Remove(model);
            });

            CollectionAssert.DoesNotContain(collection.Select(m => m.Model), model);
        }

        [Test]
        public void Remove_SingleExisting_Success()
        {
            var collection = new TrackedModelCollection<BlankAggregate>();
            var model = new BlankAggregate();
            collection.Existing(model);

            Assert.DoesNotThrow(() =>
            {
                collection.Remove(model);
            });

            CollectionAssert.Contains(collection.OfState(TrackedModelState.Removed).Select(m => m.Model), model);
        }

        [Test]
        public void Remove_MultipleExisting_Success()
        {
            var collection = new TrackedModelCollection<BlankAggregate>();
            var model1 = new BlankAggregate();
            var model2 = new BlankAggregate();
            collection.Existing(model1);
            collection.Existing(model2);

            Assert.DoesNotThrow(() =>
            {
                collection.Remove(model1);
                collection.Remove(model2);
            });

            var removedModels = collection.OfState(TrackedModelState.Removed).Select(m => m.Model).ToList();
            CollectionAssert.Contains(removedModels, model1);
            CollectionAssert.Contains(removedModels, model2);
        }

        [Test]
        public void Remove_Null_Failure()
        {
            var collection = new TrackedModelCollection<BlankAggregate>();

            Assert.Throws<ArgumentNullException>(() => { collection.Remove(null); });
        }

        [Test]
        public void Remove_Absent_Failure()
        {
            var collection = new TrackedModelCollection<BlankAggregate>();
            var model = new BlankAggregate();

            Assert.Throws<KeyNotFoundException>(() =>
            {
                collection.Remove(model);
            });
        }
    }
}