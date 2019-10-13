using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDelta.UpdateStrategies;

namespace MongoDelta.ChangeTracking
{
    class ReplaceElementChangeTracker : DocumentElementChangeTrackerBase
    {
        public ReplaceElementChangeTracker(BsonMemberMap memberMap) : base(memberMap)
        {
        }

        protected override void ApplyChangesToDefinition(UpdateDefinition updateDefinition, BsonValue originalValue, BsonValue currentValue)
        {
            updateDefinition.Set(MemberMap.ElementName, currentValue);
        }
    }
}
