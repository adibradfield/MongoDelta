using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDelta.UpdateStrategies;

namespace MongoDelta.ChangeTracking
{
    class DeltaElementChangeTracker : DocumentElementChangeTrackerBase
    {
        private readonly DocumentChangeTracker _memberChangeTracker;

        public DeltaElementChangeTracker(BsonMemberMap memberMap) : base(memberMap)
        {
            _memberChangeTracker = new DocumentChangeTracker(memberMap.MemberType);
        }

        protected override void ApplyChangesToDefinition(UpdateDefinition updateDefinition, BsonValue originalValue, BsonValue currentValue)
        {
            var originalDocument = originalValue.AsBsonDocument;
            var currentDocument = currentValue.AsBsonDocument;

            var memberUpdateDefinition = _memberChangeTracker.GetUpdatesForChanges(originalDocument, currentDocument);
            updateDefinition.Merge(MemberMap.ElementName, memberUpdateDefinition);
        }
    }
}
