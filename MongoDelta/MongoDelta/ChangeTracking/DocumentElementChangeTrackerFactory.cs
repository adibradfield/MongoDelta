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

            var updateStrategy = memberMap.ClassMap.GetUpdateStrategy(memberMap.ElementName);
            switch (updateStrategy.Type)
            {
                case DeltaUpdateConfiguration.MemberUpdateStrategyType.Normal:
                    return new ReplaceElementChangeTracker(memberMap);

                case DeltaUpdateConfiguration.MemberUpdateStrategyType.Incremental:
                    return new IncrementalElementChangeTracker(memberMap);

                case DeltaUpdateConfiguration.MemberUpdateStrategyType.HashSet:
                    return new HashSetChangeTracker(memberMap);

                case DeltaUpdateConfiguration.MemberUpdateStrategyType.DeltaSet:
                    return new DeltaSetChangeTracker(memberMap, updateStrategy.CollectionItemType);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
