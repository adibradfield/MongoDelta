using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDelta.ChangeTracking;
using MongoDelta.UnitTests.Models;
using NUnit.Framework;

namespace MongoDelta.UnitTests
{
    [TestFixture]
    public class DocumentChangeTrackerTests
    {
        [Test]
        public void Update_SingleChange_Success()
        {
            var model = GetTrackedFlatAggregate(out var trackedModel);

            model.Name = "John Smith";

            AssertModelIsReplaced(trackedModel);
        }

        [Test]
        public void Update_TwoChanges_Success()
        {
            var model = GetTrackedFlatAggregate(out var trackedModel);

            model.Name = "John Smith";
            model.Age = 25;

            AssertModelIsReplaced(trackedModel);
        }

        [Test]
        public void Update_NonDeltaSubEntityUpdated_Success()
        {
            var expectedUpdateDefinition = new BsonDocument("$set",
                new BsonDocument("NonDeltaValue", new BsonDocument("Value", "NewValue")));
            var model = GetTrackedSubEntityAggregate(out var trackedModel);

            model.NonDeltaValue.Value = "NewValue";

            AssertModelEqualsExpectedDefinition(expectedUpdateDefinition, trackedModel);
        }

        [Test]
        public void Update_NonDeltaSubEntityReplaced_Success()
        {
            var expectedUpdateDefinition = new BsonDocument("$set",
                new BsonDocument("NonDeltaValue", new BsonDocument("Value", "NewValue")));
            var model = GetTrackedSubEntityAggregate(out var trackedModel);

            model.NonDeltaValue = new SubEntityAsDeltaAggregate.NonDeltaSubEntity
            {
                Value = "NewValue"
            };

            AssertModelEqualsExpectedDefinition(expectedUpdateDefinition, trackedModel);
        }

        [Test]
        public void Update_DeltaSubEntityUpdated_Success()
        {
            var expectedUpdateDefinition = new BsonDocument("$set",
                new BsonDocument("DeltaValue.Value", "NewValue"));
            var model = GetTrackedSubEntityAggregate(out var trackedModel);

            model.DeltaValue.Value = "NewValue";

            AssertModelEqualsExpectedDefinition(expectedUpdateDefinition, trackedModel);
        }

        [Test]
        public void Update_DeltaSubEntityReplaced_Success()
        {
            var expectedUpdateDefinition = new BsonDocument("$set",
                new BsonDocument("DeltaValue.Value", "NewValue"));
            var model = GetTrackedSubEntityAggregate(out var trackedModel);

            model.DeltaValue = new SubEntityAsDeltaAggregate.DeltaSubEntity
            {
                Value = "NewValue"
            };

            AssertModelEqualsExpectedDefinition(expectedUpdateDefinition, trackedModel);
        }

        [Test]
        public void Update_DeltaSubEntityValueToNull_Success()
        {
            var expectedUpdateDefinition = new BsonDocument("$set",
                new BsonDocument("DeltaValue", BsonNull.Value));
            var model = GetTrackedSubEntityAggregate(out var trackedModel);

            model.DeltaValue = null;

            AssertModelEqualsExpectedDefinition(expectedUpdateDefinition, trackedModel);
        }

        [Test]
        public void Update_NonDeltaSubEntityValueToNull_Success()
        {
            var expectedUpdateDefinition = new BsonDocument("$set",
                new BsonDocument("NonDeltaValue", BsonNull.Value));
            var model = GetTrackedSubEntityAggregate(out var trackedModel);

            model.NonDeltaValue = null;

            AssertModelEqualsExpectedDefinition(expectedUpdateDefinition, trackedModel);
        }

        [Test]
        public void Update_DeltaSubEntityNullToValue_Success()
        {
            var expectedUpdateDefinition = new BsonDocument("$set",
                new BsonDocument("DeltaValue", new BsonDocument("Value", "NewValue")));
            var model = GetNullTrackedSubEntityAggregate(out var trackedModel);

            model.DeltaValue = new SubEntityAsDeltaAggregate.DeltaSubEntity
            {
                Value = "NewValue"
            };

            AssertModelEqualsExpectedDefinition(expectedUpdateDefinition, trackedModel);
        }

        [Test]
        public void Update_NonDeltaSubEntityNullToValue_Success()
        {
            var expectedUpdateDefinition = new BsonDocument("$set",
                new BsonDocument("NonDeltaValue", new BsonDocument("Value", "NewValue")));
            var model = GetNullTrackedSubEntityAggregate(out var trackedModel);

            model.NonDeltaValue = new SubEntityAsDeltaAggregate.NonDeltaSubEntity
            {
                Value = "NewValue"
            };

            AssertModelEqualsExpectedDefinition(expectedUpdateDefinition, trackedModel);
        }

        [Test]
        public void Update_IntegerIncrementally_Success()
        {
            var expectedUpdateDefinition = new BsonDocument("$inc",
                new BsonDocument("Integer", 12));
            var model = GetTrackedIncrementNumeralsAggregate(out var trackedModel);

            model.Integer += 12;

            AssertModelEqualsExpectedDefinition(expectedUpdateDefinition, trackedModel);
        }

        [Test]
        public void Update_LongIncrementally_Success()
        {
            var expectedUpdateDefinition = new BsonDocument("$inc",
                new BsonDocument("Long", 10567));
            var model = GetTrackedIncrementNumeralsAggregate(out var trackedModel);

            model.Long += 10567;

            AssertModelEqualsExpectedDefinition(expectedUpdateDefinition, trackedModel);
        }

        [Test]
        public void Update_DecimalIncrementally_Success()
        {
            var expectedUpdateDefinition = new BsonDocument("$inc",
                new BsonDocument("Decimal", 89.4M));
            var model = GetTrackedIncrementNumeralsAggregate(out var trackedModel);

            model.Decimal += 89.4M;

            AssertModelEqualsExpectedDefinition(expectedUpdateDefinition, trackedModel);
        }

        [Test]
        public void Update_DoubleIncrementally_Success()
        {
            var expectedUpdateDefinition = new BsonDocument("$inc",
                new BsonDocument("Double", 12.896));
            var model = GetTrackedIncrementNumeralsAggregate(out var trackedModel);

            model.Double += 12.896;

            AssertModelEqualsExpectedDefinition(expectedUpdateDefinition, trackedModel, 
                new Dictionary<string, Action<BsonValue, BsonValue>>
                {
                    { "$inc.Double", (expected, actual) =>
                        {
                            Assert.That(actual.AsDouble, Is.EqualTo(expected.AsDouble).Within(0.0000000001));
                        }
                    }
                });
        }

        [Test]
        public void Update_IntegerDecrementally_Success()
        {
            var expectedUpdateDefinition = new BsonDocument("$inc",
                new BsonDocument("Integer", -12));
            var model = GetTrackedIncrementNumeralsAggregate(out var trackedModel);

            model.Integer -= 12;

            AssertModelEqualsExpectedDefinition(expectedUpdateDefinition, trackedModel);
        }

        [Test]
        public void Update_LongDecrementally_Success()
        {
            var expectedUpdateDefinition = new BsonDocument("$inc",
                new BsonDocument("Long", -10567));
            var model = GetTrackedIncrementNumeralsAggregate(out var trackedModel);

            model.Long -= 10567;

            AssertModelEqualsExpectedDefinition(expectedUpdateDefinition, trackedModel);
        }

        [Test]
        public void Update_DecimalDecrementally_Success()
        {
            var expectedUpdateDefinition = new BsonDocument("$inc",
                new BsonDocument("Decimal", -89.4M));
            var model = GetTrackedIncrementNumeralsAggregate(out var trackedModel);

            model.Decimal -= 89.4M;

            AssertModelEqualsExpectedDefinition(expectedUpdateDefinition, trackedModel);
        }

        [Test]
        public void Update_DoubleDecrementally_Success()
        {
            var expectedUpdateDefinition = new BsonDocument("$inc",
                new BsonDocument("Double", -12.896));
            var model = GetTrackedIncrementNumeralsAggregate(out var trackedModel);

            model.Double -= 12.896;

            AssertModelEqualsExpectedDefinition(expectedUpdateDefinition, trackedModel, 
                new Dictionary<string, Action<BsonValue, BsonValue>>
                {
                    { "$inc.Double", (expected, actual) =>
                        {
                            Assert.That(actual.AsDouble, Is.EqualTo(expected.AsDouble).Within(0.0000000001));
                        }
                    }
                });
        }

        [Test]
        public void UpdateAsHashSet_SingleItemAdded_Success()
        {
            var expectedUpdateDefinition = new BsonDocument("$addToSet",
                new BsonDocument("HashSet", new BsonDocument("$each", new BsonArray(new[] {"Bananas"}))));
            var model = GetTrackedDeltaCollectionAggregate(out var trackedModel);

            model.HashSet.Add("Bananas");

            AssertModelEqualsExpectedDefinition(expectedUpdateDefinition, trackedModel);
        }

        [Test]
        public void UpdateAsHashSet_SingleItemRemoved_Success()
        {
            var expectedUpdateDefinition = new BsonDocument("$pull",
                new BsonDocument("HashSet", new BsonDocument("$in", new BsonArray(new[] {"Apples"}))));
            var model = GetTrackedDeltaCollectionAggregate(out var trackedModel);

            model.HashSet.Remove("Apples");

            AssertModelEqualsExpectedDefinition(expectedUpdateDefinition, trackedModel);
        }

        [Test]
        public void UpdateAsHashSet_TwoItemsAdded_Success()
        {
            var expectedUpdateDefinition = new BsonDocument("$addToSet",
                new BsonDocument("HashSet", new BsonDocument("$each", new BsonArray(new[] {"Bananas", "Pears"}))));
            var model = GetTrackedDeltaCollectionAggregate(out var trackedModel);

            model.HashSet.Add("Bananas");
            model.HashSet.Add("Pears");

            AssertModelEqualsExpectedDefinition(expectedUpdateDefinition, trackedModel, new Dictionary<string, Action<BsonValue, BsonValue>>()
            {
                { "$addToSet.HashSet.$each", (expectedValue, actualValue) => CollectionAssert.AreEquivalent(expectedValue.AsBsonArray, actualValue.AsBsonArray) }
            });
        }

        [Test]
        public void UpdateAsHashSet_TwoItemsRemoved_Success()
        {
            var expectedUpdateDefinition = new BsonDocument("$pull",
                new BsonDocument("HashSet", new BsonDocument("$in", new BsonArray(new[] {"Apples", "Oranges"}))));
            var model = GetTrackedDeltaCollectionAggregate(out var trackedModel);

            model.HashSet.Remove("Apples");
            model.HashSet.Remove("Oranges");

            AssertModelEqualsExpectedDefinition(expectedUpdateDefinition, trackedModel, new Dictionary<string, Action<BsonValue, BsonValue>>()
            {
                { "$pull.HashSet.$in", (expectedValue, actualValue) => CollectionAssert.AreEquivalent(expectedValue.AsBsonArray, actualValue.AsBsonArray) }
            });
        }

        [Test]
        public void UpdateAsHashSet_DuplicateItemAddedNoChange_Success()
        {
            var model = GetTrackedDeltaCollectionAggregate(out var trackedModel);

            model.HashSet.Add("Apples");

            AssertModelEqualsExpectedDefinition(null, trackedModel);
        }

        [Test]
        public void UpdateAsHashSet_DuplicateItemRemovedNoChange_Success()
        {
            var model = GetTrackedDeltaCollectionAggregate(out var trackedModel);

            model.HashSet.Add("Apples");
            model.HashSet.Remove("Apples");

            AssertModelEqualsExpectedDefinition(null, trackedModel);
        }

        #region Test Helpers

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

        private static SubEntityAsDeltaAggregate GetTrackedSubEntityAggregate(
            out TrackedModel<SubEntityAsDeltaAggregate> trackedModel)
        {
            var model = new SubEntityAsDeltaAggregate
            {
                NonDeltaValue = new SubEntityAsDeltaAggregate.NonDeltaSubEntity
                {
                    Value = "NonDelta"
                },
                DeltaValue = new SubEntityAsDeltaAggregate.DeltaSubEntity
                {
                    Value = "Delta"
                }
            };

            var trackingCollection = new TrackedModelCollection<SubEntityAsDeltaAggregate>();
            trackingCollection.Existing(model);
            trackedModel = trackingCollection.Single();
            return model;
        }

        private static SubEntityAsDeltaAggregate GetNullTrackedSubEntityAggregate(
            out TrackedModel<SubEntityAsDeltaAggregate> trackedModel)
        {
            var model = new SubEntityAsDeltaAggregate
            {
                NonDeltaValue = null,
                DeltaValue = null
            };

            var trackingCollection = new TrackedModelCollection<SubEntityAsDeltaAggregate>();
            trackingCollection.Existing(model);
            trackedModel = trackingCollection.Single();
            return model;
        }

        private static IncrementNumeralsAggregate GetTrackedIncrementNumeralsAggregate(
            out TrackedModel<IncrementNumeralsAggregate> trackedModel)
        {
            var model = new IncrementNumeralsAggregate
            {
                Name = "Name",
                Integer = 5,
                Long = 120567,
                Decimal = 65.3M,
                Double = 67384.8956
            };

            var trackingCollection = new TrackedModelCollection<IncrementNumeralsAggregate>();
            trackingCollection.Existing(model);
            trackedModel = trackingCollection.Single();
            return model;
        }

        private static DeltaCollectionAggregate GetTrackedDeltaCollectionAggregate(
            out TrackedModel<DeltaCollectionAggregate> trackedModel)
        {
            var model = new DeltaCollectionAggregate
            {
                HashSet = new List<string> {"Apples", "Oranges"}
            };

            var trackingCollection = new TrackedModelCollection<DeltaCollectionAggregate>();
            trackingCollection.Existing(model);
            trackedModel = trackingCollection.Single();
            return model;
        }

        private static void AssertModelIsReplaced(TrackedModel<FlatAggregate> trackedModel)
        {
            var changeTracker = new DocumentChangeTracker(typeof(FlatAggregate));
            var updateDefinition = changeTracker.GetUpdatesForChanges(trackedModel.OriginalDocument, trackedModel.CurrentDocument);
            var writeModel = (ReplaceOneModel<FlatAggregate>) updateDefinition.ToMongoWriteModels(FilterDefinition<FlatAggregate>.Empty).Single();

            Assert.AreEqual(trackedModel.Model.Id, writeModel.Replacement.Id);
        }

        private static void AssertModelEqualsExpectedDefinition<T>(BsonDocument expectedUpdateDefinition,
            TrackedModel<T> trackedModel, Dictionary<string, Action<BsonValue, BsonValue>> customAsserts = null) where T : class
        {
            if (customAsserts == null)
            {
                customAsserts = new Dictionary<string, Action<BsonValue, BsonValue>>();
            }

            var changeTracker = new DocumentChangeTracker(typeof(T));
            var updateDefinition = changeTracker.GetUpdatesForChanges(trackedModel.OriginalDocument, trackedModel.CurrentDocument);
            var writeModel = (UpdateOneModel<T>) updateDefinition.ToMongoWriteModels(FilterDefinition<T>.Empty).SingleOrDefault();

            if (expectedUpdateDefinition == null)
            {
                Assert.IsNull(writeModel);
                return;
            }

            Assert.IsNotNull(writeModel);
            var actualUpdateDefinition = writeModel.Update.ToBsonDocument()["Document"].ToBsonDocument();

            foreach (var customAssert in customAsserts)
            {
                PopValueFromExpectedAndActualDocuments(expectedUpdateDefinition, actualUpdateDefinition, customAssert.Key, out var expectedValue, out var actualValue);
                customAssert.Value(expectedValue, actualValue);
            }

            var expectedKeys = expectedUpdateDefinition.Elements.Select(e => e.Name).ToArray();
            var actualKeys = actualUpdateDefinition.Elements.Select(e => e.Name).ToArray();
            CollectionAssert.AreEquivalent(expectedKeys, actualKeys);

            foreach (var key in actualKeys)
            {
                CollectionAssert.AreEquivalent(expectedUpdateDefinition[key].AsBsonDocument, actualUpdateDefinition[key].AsBsonDocument);
            }
        }

        private static void PopValueFromExpectedAndActualDocuments(BsonDocument expectedUpdateDefinition,
            BsonDocument actualUpdateDefinition, string elementPath, out BsonValue expectedValue,
            out BsonValue actualValue)
        {
            BsonDocument lastExpectedDocument = null, lastActualDocument = null;
            expectedValue = expectedUpdateDefinition.AsBsonValue;
            actualValue = actualUpdateDefinition.AsBsonValue;
            string lastPathPart = null;
            foreach (var pathPart in elementPath.Split('.'))
            {
                lastExpectedDocument = expectedValue.AsBsonDocument;
                lastActualDocument = actualValue.AsBsonDocument;

                expectedValue = expectedValue[pathPart];
                actualValue = actualValue[pathPart];

                lastPathPart = pathPart;
            }

            if (lastExpectedDocument != null) lastExpectedDocument.Remove(lastPathPart);
            if (lastActualDocument != null) lastActualDocument.Remove(lastPathPart);
        }

        #endregion
    }
}
