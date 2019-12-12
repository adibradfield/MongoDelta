using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDelta.UpdateStrategies;

namespace MongoDelta.ChangeTracking.ElementChangeTrackers
{
    abstract class DocumentElementChangeTrackerBase : IDocumentElementChangeTracker
    {
        protected readonly BsonMemberMap MemberMap;

        protected DocumentElementChangeTrackerBase(BsonMemberMap memberMap)
        {
            MemberMap = memberMap;
        }

        public void ApplyChangesToDefinition(UpdateDefinition updateDefinition, BsonDocument original, BsonDocument current)
        {
            var originalValue = original[MemberMap.ElementName];
            var currentValue = current[MemberMap.ElementName];

            if (originalValue == currentValue) return;

            if (currentValue == BsonNull.Value)
            {
                updateDefinition.Set(MemberMap.ElementName, BsonNull.Value);
            }
            else if (originalValue == BsonNull.Value)
            {
                updateDefinition.Set(MemberMap.ElementName, currentValue);
            }
            else
            {
                ApplyChangesToDefinition(updateDefinition, originalValue, currentValue);
            }
        }

        protected abstract void ApplyChangesToDefinition(UpdateDefinition updateDefinition,
            BsonValue originalValue, BsonValue currentValue);
    }
}
