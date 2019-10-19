using System;
using MongoDB.Bson.Serialization;
using MongoDelta.ChangeTracking.ElementChangeTrackers;
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

            switch (memberMap.ClassMap.GetUpdateStrategy(memberMap.ElementName))
            {
                case DeltaUpdateConfiguration.MemberUpdateStrategy.Normal:
                    return new ReplaceElementChangeTracker(memberMap);

                case DeltaUpdateConfiguration.MemberUpdateStrategy.Incremental:
                    return new IncrementalElementChangeTracker(memberMap);

                case DeltaUpdateConfiguration.MemberUpdateStrategy.HashSet:
                    return new HashSetChangeTracker(memberMap);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
