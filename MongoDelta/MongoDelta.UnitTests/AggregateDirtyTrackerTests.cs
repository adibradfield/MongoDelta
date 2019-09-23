using System;
using System.Collections.Generic;
using MongoDelta.ChangeTracking.DirtyTracking;
using MongoDelta.UnitTests.Models;
using NUnit.Framework;

namespace MongoDelta.UnitTests
{
    [TestFixture]
    public class AggregateDirtyTrackerTests
    {
        [Test]
        public void IsDirty_NothingChanged_False()
        {
            var model = new FlatAggregate();
            var tracker = new AggregateDirtyTracker<FlatAggregate>(model);
            Assert.IsFalse(tracker.IsDirty);
        }

        [Test]
        public void IsDirty_ChangeString_True()
        {
            var model = new FlatAggregate();
            var tracker = new AggregateDirtyTracker<FlatAggregate>(model);

            model.Name = "Jane Doe";

            Assert.IsTrue(tracker.IsDirty);
        }

        [Test]
        public void IsDirty_ChangeInt_True()
        {
            var model = new FlatAggregate();
            var tracker = new AggregateDirtyTracker<FlatAggregate>(model);

            model.Age = 22;

            Assert.IsTrue(tracker.IsDirty);
        }

        [Test]
        public void IsDirty_ChangeDateTime_True()
        {
            var model = new FlatAggregate();
            var tracker = new AggregateDirtyTracker<FlatAggregate>(model);

            model.DateOfBirth = new DateTime(2000, 01, 01);

            Assert.IsTrue(tracker.IsDirty);
        }

        [Test]
        public void IsDirty_SubEntityChanged_True()
        {
            var model = new SubEntityAggregate();
            var tracker = new AggregateDirtyTracker<SubEntityAggregate>(model);

            model.Entity.Name = "Jane Doe";

            Assert.IsTrue(tracker.IsDirty);
        }

        [Test]
        public void IsDirty_SubEntityReplaced_True()
        {
            var model = new SubEntityAggregate();
            var tracker = new AggregateDirtyTracker<SubEntityAggregate>(model);

            model.Entity = new SubEntityAggregate.SubEntity()
            {
                Name = "Jane Doe"
            };

            Assert.IsTrue(tracker.IsDirty);
        }

        [Test]
        public void IsDirty_ListUnchanged_False()
        {
            var model = new ListAggregate();
            var tracker = new AggregateDirtyTracker<ListAggregate>(model);

            Assert.IsFalse(tracker.IsDirty);
        }

        [Test]
        public void IsDirty_ListChanged_True()
        {
            var model = new ListAggregate();
            var tracker = new AggregateDirtyTracker<ListAggregate>(model);

            model.Names.Add(new ListAggregate.PersonName("Joan"));

            Assert.IsTrue(tracker.IsDirty);
        }

        [Test]
        public void IsDirty_ListReplaced_True()
        {
            var model = new ListAggregate();
            var tracker = new AggregateDirtyTracker<ListAggregate>(model);

            model.Names = new List<ListAggregate.PersonName>(){new ListAggregate.PersonName("Joan")};

            Assert.IsTrue(tracker.IsDirty);
        }

        [Test]
        public void IsDirty_DictionaryUnchanged_False()
        {
            var model = new DictionaryAggregate();
            var tracker = new AggregateDirtyTracker<DictionaryAggregate>(model);

            Assert.IsFalse(tracker.IsDirty);
        }

        [Test]
        public void IsDirty_DictionaryChanged_True()
        {
            var model = new DictionaryAggregate();
            var tracker = new AggregateDirtyTracker<DictionaryAggregate>(model);

            model.Ages.Add("Joan", new DictionaryAggregate.PersonAge(52));

            Assert.IsTrue(tracker.IsDirty);
        }

        [Test]
        public void IsDirty_DictionaryReplaced_True()
        {
            var model = new DictionaryAggregate();
            var tracker = new AggregateDirtyTracker<DictionaryAggregate>(model);

            model.Ages = new Dictionary<string, DictionaryAggregate.PersonAge>()
            {
                { "Joan", new DictionaryAggregate.PersonAge(52) }
            };

            Assert.IsTrue(tracker.IsDirty);
        }
    }
}