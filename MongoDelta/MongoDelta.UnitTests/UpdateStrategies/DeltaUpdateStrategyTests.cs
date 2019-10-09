using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDelta.ChangeTracking;
using MongoDelta.UnitTests.Models;
using MongoDelta.UpdateStrategies;
using NUnit.Framework;

namespace MongoDelta.UnitTests.UpdateStrategies
{
    [TestFixture]
    public class DeltaUpdateStrategyTests
    {
        [Test]
        public void Update_SingleChange_Success()
        {
            var expectedUpdateDefinition = new BsonDocument("$set", new BsonDocument("Name", "John Smith"));
            var model = GetTrackedFlatAggregate(out var trackedModel);

            model.Name = "John Smith";

            var strategy = new DeltaUpdateStrategy<FlatAggregate>();
            var writeModel = strategy.GetWriteModelForUpdate(trackedModel);

            AssertModelEqualsExpectedDefinition(expectedUpdateDefinition, writeModel);
        }

        [Test]
        public void Update_TwoChanges_Success()
        {
            var expectedUpdateDefinition = new BsonDocument("$set", new BsonDocument
            {
                new BsonElement("Age", 25),
                new BsonElement("Name", "John Smith"),
            });
            var model = GetTrackedFlatAggregate(out var trackedModel);

            model.Name = "John Smith";
            model.Age = 25;

            var strategy = new DeltaUpdateStrategy<FlatAggregate>();
            var writeModel = strategy.GetWriteModelForUpdate(trackedModel);

            AssertModelEqualsExpectedDefinition(expectedUpdateDefinition, writeModel);
        }

        [Test]
        public void Update_NonDeltaSubEntityUpdated_Success()
        {
            var expectedUpdateDefinition = new BsonDocument("$set",
                new BsonDocument("NonDeltaValue", new BsonDocument("Value", "NewValue")));
            var model = GetTrackedSubEntityAggregate(out var trackedModel);

            model.NonDeltaValue.Value = "NewValue";

            var strategy = new DeltaUpdateStrategy<SubEntityAsDeltaAggregate>();
            var writeModel = strategy.GetWriteModelForUpdate(trackedModel);

            AssertModelEqualsExpectedDefinition(expectedUpdateDefinition, writeModel);
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

            var strategy = new DeltaUpdateStrategy<SubEntityAsDeltaAggregate>();
            var writeModel = strategy.GetWriteModelForUpdate(trackedModel);

            AssertModelEqualsExpectedDefinition(expectedUpdateDefinition, writeModel);
        }

        [Test]
        public void Update_DeltaSubEntityUpdated_Success()
        {
            var expectedUpdateDefinition = new BsonDocument("$set",
                new BsonDocument("DeltaValue.Value", "NewValue"));
            var model = GetTrackedSubEntityAggregate(out var trackedModel);

            model.DeltaValue.Value = "NewValue";

            var strategy = new DeltaUpdateStrategy<SubEntityAsDeltaAggregate>();
            var writeModel = strategy.GetWriteModelForUpdate(trackedModel);

            AssertModelEqualsExpectedDefinition(expectedUpdateDefinition, writeModel);
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

            var strategy = new DeltaUpdateStrategy<SubEntityAsDeltaAggregate>();
            var writeModel = strategy.GetWriteModelForUpdate(trackedModel);

            AssertModelEqualsExpectedDefinition(expectedUpdateDefinition, writeModel);
        }

        [Test]
        public void Update_DeltaSubEntityValueToNull_Success()
        {
            var expectedUpdateDefinition = new BsonDocument("$set",
                new BsonDocument("DeltaValue", BsonNull.Value));
            var model = GetTrackedSubEntityAggregate(out var trackedModel);

            model.DeltaValue = null;

            var strategy = new DeltaUpdateStrategy<SubEntityAsDeltaAggregate>();
            var writeModel = strategy.GetWriteModelForUpdate(trackedModel);

            AssertModelEqualsExpectedDefinition(expectedUpdateDefinition, writeModel);
        }

        [Test]
        public void Update_NonDeltaSubEntityValueToNull_Success()
        {
            var expectedUpdateDefinition = new BsonDocument("$set",
                new BsonDocument("NonDeltaValue", BsonNull.Value));
            var model = GetTrackedSubEntityAggregate(out var trackedModel);

            model.NonDeltaValue = null;

            var strategy = new DeltaUpdateStrategy<SubEntityAsDeltaAggregate>();
            var writeModel = strategy.GetWriteModelForUpdate(trackedModel);

            AssertModelEqualsExpectedDefinition(expectedUpdateDefinition, writeModel);
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

            var strategy = new DeltaUpdateStrategy<SubEntityAsDeltaAggregate>();
            var writeModel = strategy.GetWriteModelForUpdate(trackedModel);

            AssertModelEqualsExpectedDefinition(expectedUpdateDefinition, writeModel);
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

            var strategy = new DeltaUpdateStrategy<SubEntityAsDeltaAggregate>();
            var writeModel = strategy.GetWriteModelForUpdate(trackedModel);

            AssertModelEqualsExpectedDefinition(expectedUpdateDefinition, writeModel);
        }

        [Test]
        public void Update_IntegerIncrementally_Success()
        {
            var expectedUpdateDefinition = new BsonDocument("$inc",
                new BsonDocument("Integer", 12));
            var model = GetTrackedIncrementNumeralsAggregate(out var trackedModel);

            model.Integer += 12;

            var strategy = new DeltaUpdateStrategy<IncrementNumeralsAggregate>();
            var writeModel = strategy.GetWriteModelForUpdate(trackedModel);

            AssertModelEqualsExpectedDefinition(expectedUpdateDefinition, writeModel);
        }

        [Test]
        public void Update_LongIncrementally_Success()
        {
            var expectedUpdateDefinition = new BsonDocument("$inc",
                new BsonDocument("Long", 10567));
            var model = GetTrackedIncrementNumeralsAggregate(out var trackedModel);

            model.Long += 10567;

            var strategy = new DeltaUpdateStrategy<IncrementNumeralsAggregate>();
            var writeModel = strategy.GetWriteModelForUpdate(trackedModel);

            AssertModelEqualsExpectedDefinition(expectedUpdateDefinition, writeModel);
        }

        [Test]
        public void Update_DecimalIncrementally_Success()
        {
            var expectedUpdateDefinition = new BsonDocument("$inc",
                new BsonDocument("Decimal", 89.4M));
            var model = GetTrackedIncrementNumeralsAggregate(out var trackedModel);

            model.Decimal += 89.4M;

            var strategy = new DeltaUpdateStrategy<IncrementNumeralsAggregate>();
            var writeModel = strategy.GetWriteModelForUpdate(trackedModel);

            AssertModelEqualsExpectedDefinition(expectedUpdateDefinition, writeModel);
        }

        [Test]
        public void Update_DoubleIncrementally_Success()
        {
            var expectedUpdateDefinition = new BsonDocument("$inc",
                new BsonDocument("Double", 12.896));
            var model = GetTrackedIncrementNumeralsAggregate(out var trackedModel);

            model.Double += 12.896;

            var strategy = new DeltaUpdateStrategy<IncrementNumeralsAggregate>();
            var writeModel = strategy.GetWriteModelForUpdate(trackedModel);

            AssertModelEqualsExpectedDefinition(expectedUpdateDefinition, writeModel, 
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

            var strategy = new DeltaUpdateStrategy<IncrementNumeralsAggregate>();
            var writeModel = strategy.GetWriteModelForUpdate(trackedModel);

            AssertModelEqualsExpectedDefinition(expectedUpdateDefinition, writeModel);
        }

        [Test]
        public void Update_LongDecrementally_Success()
        {
            var expectedUpdateDefinition = new BsonDocument("$inc",
                new BsonDocument("Long", -10567));
            var model = GetTrackedIncrementNumeralsAggregate(out var trackedModel);

            model.Long -= 10567;

            var strategy = new DeltaUpdateStrategy<IncrementNumeralsAggregate>();
            var writeModel = strategy.GetWriteModelForUpdate(trackedModel);

            AssertModelEqualsExpectedDefinition(expectedUpdateDefinition, writeModel);
        }

        [Test]
        public void Update_DecimalDecrementally_Success()
        {
            var expectedUpdateDefinition = new BsonDocument("$inc",
                new BsonDocument("Decimal", -89.4M));
            var model = GetTrackedIncrementNumeralsAggregate(out var trackedModel);

            model.Decimal -= 89.4M;

            var strategy = new DeltaUpdateStrategy<IncrementNumeralsAggregate>();
            var writeModel = strategy.GetWriteModelForUpdate(trackedModel);

            AssertModelEqualsExpectedDefinition(expectedUpdateDefinition, writeModel);
        }

        [Test]
        public void Update_DoubleDecrementally_Success()
        {
            var expectedUpdateDefinition = new BsonDocument("$inc",
                new BsonDocument("Double", -12.896));
            var model = GetTrackedIncrementNumeralsAggregate(out var trackedModel);

            model.Double -= 12.896;

            var strategy = new DeltaUpdateStrategy<IncrementNumeralsAggregate>();
            var writeModel = strategy.GetWriteModelForUpdate(trackedModel);

            AssertModelEqualsExpectedDefinition(expectedUpdateDefinition, writeModel, 
                new Dictionary<string, Action<BsonValue, BsonValue>>
                {
                    { "$inc.Double", (expected, actual) =>
                        {
                            Assert.That(actual.AsDouble, Is.EqualTo(expected.AsDouble).Within(0.0000000001));
                        }
                    }
                });
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

        private static void AssertModelEqualsExpectedDefinition<T>(BsonDocument expectedUpdateDefinition,
            WriteModel<T> writeModel, Dictionary<string, Action<BsonValue, BsonValue>> customAsserts = null)
        {
            if (customAsserts == null)
            {
                customAsserts = new Dictionary<string, Action<BsonValue, BsonValue>>();
            }

            var updateDefinition = ((UpdateOneModel<T>) writeModel).Update;
            var actualUpdateDefinition = updateDefinition.ToBsonDocument()["Document"].ToBsonDocument();

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
    }
}
