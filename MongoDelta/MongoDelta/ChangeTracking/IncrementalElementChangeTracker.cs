using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDelta.UpdateStrategies;

namespace MongoDelta.ChangeTracking
{
    class IncrementalElementChangeTracker : DocumentElementChangeTrackerBase
    {
        public IncrementalElementChangeTracker(BsonMemberMap memberMap) : base(memberMap)
        {
        }

        protected override void ApplyChangesToDefinition(UpdateDefinition updateDefinition, BsonValue originalValue, BsonValue currentValue)
        {
            var difference = GetValueDifferenceAsBsonValue(originalValue, currentValue);
            updateDefinition.Increment(MemberMap.ElementName, difference);
        }

        private static BsonValue GetValueDifferenceAsBsonValue(BsonValue originalValue, BsonValue currentValue)
        {
            if (currentValue.BsonType != originalValue.BsonType)
            {
                throw new InvalidOperationException("BSON type of current value does not equal the original value");
            }

            BsonValue incrementBy;
            switch (currentValue.BsonType)
            {
                case BsonType.Double:
                    incrementBy = new BsonDouble(currentValue.AsDouble - originalValue.AsDouble);
                    break;
                case BsonType.Int32:
                    incrementBy = new BsonInt32(currentValue.AsInt32 - originalValue.AsInt32);
                    break;
                case BsonType.Int64:
                    incrementBy = new BsonInt64(currentValue.AsInt64 - originalValue.AsInt64);
                    break;
                case BsonType.Decimal128:
                    incrementBy = new BsonDecimal128(currentValue.AsDecimal - originalValue.AsDecimal);
                    break;
                default:
                    throw new InvalidOperationException(
                        $"BSON type {currentValue.BsonType} cannot be incrementally updated");
            }

            return incrementBy;
        }
    }
}
