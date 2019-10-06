using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MongoDB.Bson.Serialization;

namespace MongoDelta.ChangeTracking.DirtyTracking
{
    internal class AggregateDirtyTracker<TAggregate> : IDirtyTracker, IObjectDirtyTracker where TAggregate : class
    {
        private readonly TAggregate _aggregate;
        private readonly Collection<IMemberDirtyTrackerTemplate> _memberDirtyTrackers;

        public AggregateDirtyTracker(TAggregate aggregate)
        {
            _aggregate = aggregate;
            _memberDirtyTrackers = new Collection<IMemberDirtyTrackerTemplate>(GetDirtyTrackers(aggregate));
        }

        public bool IsDirty => MemberTrackers.Any(t => t.IsDirty);

        public IReadOnlyCollection<IMemberDirtyTracker> MemberTrackers =>
            Array.AsReadOnly(_memberDirtyTrackers.ToTrackers(_aggregate).ToArray());

        private List<IMemberDirtyTrackerTemplate> GetDirtyTrackers(TAggregate aggregate)
        {
            var mongoMapping = BsonClassMap.LookupClassMap(typeof(TAggregate));
            return mongoMapping.AllMemberMaps
                .Select(map => DirtyTrackerProvider.GetTrackerTemplateForMember(aggregate, map))
                .ToList();
        }
    }
}
