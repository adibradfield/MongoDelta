using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDelta.UpdateStrategies;

namespace MongoDelta.ChangeTracking.ElementChangeTrackers
{
    class HashSetChangeTracker : DocumentElementChangeTrackerBase
    {
        public HashSetChangeTracker(BsonMemberMap memberMap) : base(memberMap)
        {
        }

        protected override void ApplyChangesToDefinition(UpdateDefinition updateDefinition, BsonValue originalValue, BsonValue currentValue)
        {
            var originalValues = originalValue.AsBsonArray.Values.ToArray();
            var currentValues = currentValue.AsBsonArray.Values.ToArray();

            var addedValues = currentValues.Where(v => !originalValues.Contains(v)).Distinct().ToArray();
            var removedValues = originalValues.Where(v => !currentValues.Contains(v)).Distinct().ToArray();

            updateDefinition.UpdateHashSet(MemberMap.ElementName, addedValues, removedValues);
        }
    }
}
