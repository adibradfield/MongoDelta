using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace MongoDelta.ChangeTracking.DirtyTracking
{
    class SubObjectDirtyTrackerTemplate : IMemberDirtyTrackerTemplate
    {
        private readonly BsonMemberMap _map;
        private readonly BsonValue _originalValue;
        private readonly List<IMemberDirtyTrackerTemplate> _trackerTemplates;

        public SubObjectDirtyTrackerTemplate(object aggregate, BsonMemberMap map)
        {
            _map = map;
            _originalValue = _map.GetSerializer().ToBsonValue(_map.Getter(aggregate));
            _trackerTemplates = GetDirtyTrackerTemplates(_map.MemberType, _map.Getter(aggregate));
        }

        public IMemberDirtyTracker ToDirtyTracker(object aggregate)
        {
            return new SubObjectDirtyTracker(aggregate, _map, _originalValue, _trackerTemplates.ToTrackers(_map.Getter(aggregate)).ToList());
        }

        private List<IMemberDirtyTrackerTemplate> GetDirtyTrackerTemplates(Type memberType, object aggregate)
        {
            var mongoMapping = BsonClassMap.LookupClassMap(memberType);
            return mongoMapping.AllMemberMaps.Select(map => DirtyTrackerProvider.GetTrackerTemplateForMember(aggregate, map)).ToList();
        }
    }
}
