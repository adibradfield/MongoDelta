using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MongoDB.Bson.Serialization;

namespace MongoDelta.ChangeTracking.DirtyTracking
{
    class AggregateDirtyTracker<TAggregate> : IDirtyTracker where TAggregate : class
    {
        private readonly Collection<IDirtyTracker> _memberDirtyTrackers;

        public AggregateDirtyTracker(TAggregate aggregate)
        {
            _memberDirtyTrackers = new Collection<IDirtyTracker>(GetDirtyTrackers(aggregate));
        }

        public bool IsDirty => _memberDirtyTrackers.Any(t => t.IsDirty);

        private List<IDirtyTracker> GetDirtyTrackers(TAggregate aggregate)
        {
            var mongoMapping = BsonClassMap.LookupClassMap(typeof(TAggregate));
            return mongoMapping.AllMemberMaps.Select(map => CreateTrackerForMemberMap(aggregate, map)).ToList();
        }

        private IDirtyTracker CreateTrackerForMemberMap(TAggregate aggregate, BsonMemberMap map)
        {
            return new MemberDirtyTracker<TAggregate>(aggregate, map);
        }
    }
}
