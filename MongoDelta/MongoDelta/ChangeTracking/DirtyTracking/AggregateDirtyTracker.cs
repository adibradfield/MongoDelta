using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MongoDB.Bson.Serialization;

namespace MongoDelta.ChangeTracking.DirtyTracking
{
    internal class AggregateDirtyTracker<TAggregate> : IDirtyTracker where TAggregate : class
    {
        private readonly Collection<IMemberDirtyTracker> _memberDirtyTrackers;

        public AggregateDirtyTracker(TAggregate aggregate)
        {
            _memberDirtyTrackers = new Collection<IMemberDirtyTracker>(GetDirtyTrackers(aggregate));
        }

        public bool IsDirty => _memberDirtyTrackers.Any(t => t.IsDirty);

        public IReadOnlyCollection<IMemberDirtyTracker> MemberTrackers => Array.AsReadOnly(_memberDirtyTrackers.ToArray());

        private List<IMemberDirtyTracker> GetDirtyTrackers(TAggregate aggregate)
        {
            var mongoMapping = BsonClassMap.LookupClassMap(typeof(TAggregate));
            return mongoMapping.AllMemberMaps.Select(map => CreateTrackerForMemberMap(aggregate, map)).ToList();
        }

        private IMemberDirtyTracker CreateTrackerForMemberMap(TAggregate aggregate, BsonMemberMap map)
        {
            return new MemberDirtyTracker<TAggregate>(aggregate, map);
        }
    }
}
