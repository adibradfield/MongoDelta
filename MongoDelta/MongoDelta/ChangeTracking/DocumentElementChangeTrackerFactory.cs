using MongoDB.Bson.Serialization;
using MongoDelta.Mapping;

namespace MongoDelta.ChangeTracking
{
    class DocumentElementChangeTrackerFactory
    {
        public IDocumentElementChangeTracker GetChangeTrackerForElement(BsonMemberMap memberMap)
        {
            var memberClassMap = BsonClassMap.LookupClassMap(memberMap.MemberType);
            if (memberClassMap.ShouldUseDeltaUpdateStrategy())
            {
                return new DeltaElementChangeTracker(memberMap);
            }

            if (memberMap.ClassMap.ShouldUpdateIncrementally(memberMap.ElementName))
            {
                return new IncrementalElementChangeTracker(memberMap);
            }

            return new ReplaceElementChangeTracker(memberMap);
        }
    }
}
