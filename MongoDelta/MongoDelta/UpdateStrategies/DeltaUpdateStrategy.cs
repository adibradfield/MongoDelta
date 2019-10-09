using System;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDelta.ChangeTracking;
using MongoDelta.ChangeTracking.DirtyTracking;
using MongoDelta.Mapping;
using MongoDelta.MongoDbHelpers;

namespace MongoDelta.UpdateStrategies
{
    internal class DeltaUpdateStrategy<T> : UpdateStrategy<T> where T : class
    {
        public override WriteModel<T> GetWriteModelForUpdate(TrackedModel<T> trackedModel)
        {
            var updateDefinition = GetUpdateDefinitionForObject(trackedModel.DirtyTracker, typeof(T), trackedModel.Model);
            return new UpdateOneModel<T>(GenericBsonFilters.MatchSingleById(trackedModel.Model), 
                updateDefinition.ToMongoUpdateDefinition<T>());
        }

        private DeltaUpdateDefinition GetUpdateDefinitionForObject(IObjectDirtyTracker dirtyTracker, Type objectType, object aggregate)
        {
            var updateDefinition = new DeltaUpdateDefinition();
            var classMap = BsonClassMap.LookupClassMap(objectType);
            foreach (var memberTracker in dirtyTracker.MemberTrackers.Where(t => t.IsDirty))
            {
                if (classMap.ShouldUpdateIncrementally(memberTracker.ElementName))
                {
                    var incrementBy = GetValueDifferenceAsBsonValue(memberTracker);
                    updateDefinition.Increment(memberTracker.ElementName, incrementBy);
                    continue;
                }

                var memberMap = classMap.GetMemberMapForElement(memberTracker.ElementName);
                var memberClassMap = BsonClassMap.LookupClassMap(memberMap.MemberType);

                if (memberClassMap.ShouldUseDeltaUpdateStrategy())
                {
                    var objectTracker = (IObjectDirtyTracker) memberTracker;
                    var model = memberMap.Getter(aggregate);

                    if (model == null)
                    {
                        updateDefinition.Set(memberTracker.ElementName, BsonNull.Value);
                        continue;
                    }

                    if (memberTracker.OriginalValue == BsonNull.Value)
                    {
                        updateDefinition.Set(memberTracker.ElementName, memberTracker.CurrentValue);
                        continue;
                    }

                    var subUpdateDefinition = GetUpdateDefinitionForObject(objectTracker, memberMap.MemberType, model);
                    updateDefinition.Merge(memberTracker.ElementName, subUpdateDefinition);
                }
                else
                {
                    updateDefinition.Set(memberTracker.ElementName, memberTracker.CurrentValue);
                }
            }
            return updateDefinition;
        }

        private static BsonValue GetValueDifferenceAsBsonValue(IMemberDirtyTracker memberTracker)
        {
            BsonValue currentValue = memberTracker.CurrentValue, originalValue = memberTracker.OriginalValue;
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
                        $"BSON type {memberTracker.CurrentValue.BsonType} cannot be incrementally updated");
            }

            return incrementBy;
        }
    }
}
